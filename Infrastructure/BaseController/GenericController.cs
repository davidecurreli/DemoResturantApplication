using System.Net;
using System.Reflection;
using Domain;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser;

namespace Infrastructure.BaseController;

public class GenericController<TEntity>(IGenericCoreService<TEntity> dataService) : ControllerBase where TEntity : BaseEntity
{
    protected readonly IGenericCoreService<TEntity> _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));

    [HttpGet]
    [Route("Get")]
    public virtual IActionResult Get(ODataQueryOptions<TEntity> options)
    {
        IQueryable<TEntity> query;
        if (options.SelectExpand?.RawExpand != null)
            query = _dataService.GetQueryable(includes: options.SelectExpand.RawExpand);
        else
            query = _dataService.GetQueryable();

        // Apply filter
        if (options.Filter != null)
        {
            var filterExpression = ControllerQueryHelper.GetFilterExpression<TEntity>(options.Filter);
            if (filterExpression != null)
                query = query.Where(filterExpression);
        }

        // Apply ordering
        if (options.OrderBy != null)
        {
            foreach (var orderByClause in options.OrderBy.OrderByNodes)
            {
                if (orderByClause is OrderByPropertyNode orderByPropertyNode)
                {
                    var propertyName = orderByPropertyNode.Property.Name;
                    var direction = orderByPropertyNode.Direction;

                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        var orderByExpression = ControllerQueryHelper.GetOrderByExpression<TEntity>(propertyName);
                        query = direction == OrderByDirection.Ascending
                            ? query.OrderBy(orderByExpression)
                            : query.OrderByDescending(orderByExpression);
                    }
                }
            }
        }

        // Handle select and expand
        if (options.SelectExpand != null)
        {
            var selectColumnsName = GetSelectColumnNames(options.SelectExpand, typeof(TEntity));

            if (selectColumnsName.Count == 0)
                return BadRequest("Select or Expand clause is empty");

            var selectClause = ControllerQueryHelper.BuildSelectClause<object, TEntity>(selectColumnsName, query);
            var result = query.Select(selectClause).ToList();

            if (result == null || result.Count == 0)
                return Ok(new { items = Array.Empty<object>() });

            var formattedResult = FormatResult(result, selectColumnsName);

            return ReturnFormattedResult(formattedResult, options);
        }

        // Handle count
        if (options.Count != null)
        {
            var totalCount = query.Count();
            var pagedResult = ApplyPaging(query, options).ToList();

            return Ok(new { items = pagedResult, count = totalCount });
        }

        // Default case
        var finalResult = ApplyPaging(query, options).ToList();

        if (finalResult == null || finalResult.Count == 0)
            return Ok(new { items = Array.Empty<TEntity>() });

        return Ok(new { items = finalResult });
    }

    [HttpGet]
    [Route("GetById/{id}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public virtual async Task<ActionResult<TEntity>> GetById(int id)
    {
        TEntity? item = await _dataService.GetById(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Route("Insert")]
    [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public virtual async Task<ActionResult> Insert(TEntity? item)
    {
        if (item == null)
            return BadRequest();

        try
        {
            TEntity result = await _dataService.Insert(item);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to insert the item: {ex.Message}");
        }
    }

    [HttpPut]
    [Route("Update")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public virtual IActionResult Update(TEntity? item)
    {
        if (item == null)
            return BadRequest();

        try
        {
            int updateCount = _dataService.Update(item);
            return updateCount > 0 ? NoContent() : BadRequest("Failed to update the item.");
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred while updating the item: {ex.Message}");
        }
    }

    [HttpDelete]
    [Route("DeleteById/{id}")]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public virtual async Task<IActionResult> DeleteById(int id)
    {
        try
        {
            bool isDeleted = await _dataService.Delete(id);
            return isDeleted ? Ok(true) : NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while deleting the item: {ex.Message}");
        }
    }

    [HttpDelete]
    [Route("Delete")]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public virtual IActionResult Delete(TEntity item)
    {
        if (item == null)
            return BadRequest();

        bool isDeleted = _dataService.Delete(item);
        return isDeleted ? Ok(true) : NotFound();
    }

    private static List<string> GetSelectColumnNames(SelectExpandQueryOption selectExpand, Type entityType)
    {
        var selectColumnsName = new List<string>();

        if (selectExpand.RawSelect != null)
        {
            selectColumnsName.AddRange(selectExpand.RawSelect.Split(','));
        }
        else if (selectExpand.RawExpand != null)
        {
            var expandProperties = selectExpand.RawExpand.Split(',').ToList();
            var entityProperties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            foreach (var property in entityProperties)
            {
                if (expandProperties.Contains(property.Name, StringComparer.InvariantCultureIgnoreCase))
                {
                    selectColumnsName.Add(property.Name);
                    continue;
                }

                if (!property.CustomAttributes.Any(attr => attr.AttributeType.Name == "JsonIgnoreAttribute"))
                {
                    selectColumnsName.Add(property.Name);
                }
            }
        }

        return selectColumnsName;
    }

    private static List<IDictionary<string, object>> FormatResult(List<object> result, List<string> selectColumnsName)
    {
        var formattedResult = new List<IDictionary<string, object>>();

        foreach (var row in result)
        {
            var convertedRow = new Dictionary<string, object>();
            foreach (var propName in selectColumnsName)
            {
                var propertyValue = ControllerQueryHelper.GetFieldValue<object>(row, propName);
                if (propertyValue != null)
                {
                    convertedRow[propName.ToLower()] = propertyValue;
                }
            }
            formattedResult.Add(convertedRow);
        }

        return formattedResult;
    }

    private IActionResult ReturnFormattedResult(List<IDictionary<string, object>> result, ODataQueryOptions<TEntity> options)
    {
        var pagedResult = ApplyPaging(result.AsQueryable(), options);

        if (options.Count != null)
        {
            return Ok(new { items = pagedResult, count = result.Count });
        }

        return Ok(new { items = pagedResult });
    }

    private static IQueryable<T> ApplyPaging<T>(IQueryable<T> query, ODataQueryOptions<TEntity> options)
    {
        if (options.Skip != null)
            query = query.Skip(options.Skip.Value);

        if (options.Top != null)
            query = query.Take(options.Top.Value);

        return query;
    }

}