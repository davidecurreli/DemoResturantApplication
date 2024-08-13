using Domain;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser;
using System.Dynamic;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.BaseController;
public class GenericController<TEntity>(IGenericCoreService<TEntity> dataService) : ControllerBase where TEntity : BaseEntity
{
    public readonly IGenericCoreService<TEntity> _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
    private readonly JsonSerializerOptions _jsonSettings = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    [HttpGet]
    [Route("Get")]
    public virtual IActionResult Get(ODataQueryOptions<TEntity> options)
    {
        IQueryable<TEntity> query;
        if (options.SelectExpand is not null && options.SelectExpand.RawExpand is not null)
            query = _dataService.GetQueryable(includes: options.SelectExpand.RawExpand);
        else
            query = _dataService.GetQueryable();


        if (options.Filter is not null)
        {
            var filterExpression = ControllerQueryHelper.GetFilterExpression<TEntity>(options.Filter);
            if (filterExpression is not null)
                query = query.Where(filterExpression);
        }

        if (options.OrderBy != null)
        {
            foreach (var orderByClause in options.OrderBy.OrderByNodes)
            {
                var orderByPropertyNode = orderByClause as OrderByPropertyNode ?? throw new Exception("orderByClause not valid");

                var propertyName = orderByPropertyNode.Property.Name;
                var direction = orderByPropertyNode.Direction;

                if (!string.IsNullOrEmpty(propertyName))
                {
                    var orderByExpression = ControllerQueryHelper.GetOrderByExpression<TEntity>(propertyName);
                    if (direction == OrderByDirection.Ascending)
                        query = query.OrderBy(orderByExpression);
                    else
                        query = query.OrderByDescending(orderByExpression);
                }
            }
        }

        if (options.SelectExpand is not null)
        {
            var selectColumnsName = new List<string>();
            if (options.SelectExpand.RawSelect is not null)
                selectColumnsName = [.. options.SelectExpand.RawSelect.Split(',')];
            else if (options.SelectExpand.RawExpand is not null)
            {
                var expandProperties = options.SelectExpand.RawExpand.Split(',').ToList();
                var entityProperties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                foreach (var property in entityProperties)
                {
                    if (expandProperties.Contains(property.Name, StringComparer.InvariantCultureIgnoreCase))
                    {
                        selectColumnsName.Add(property.Name);
                        continue;
                    }

                    bool propertyToJsonIgnore = false;
                    foreach (var propertyAttribute in property.CustomAttributes)
                    {
                        var attributeName = propertyAttribute.AttributeType.Name;
                        if (attributeName == "JsonIgnoreAttribute")
                        {
                            propertyToJsonIgnore = true;
                            break;
                        }
                    }
                    if (!propertyToJsonIgnore)
                        selectColumnsName.Add(property.Name);
                }
            }

            if (selectColumnsName.Count == 0)
                return BadRequest("Select or Expand clause is empty");

            var selectClause = ControllerQueryHelper.BuildSelectClause<object, TEntity>(selectColumnsName, query);
            var anonymousData = query.Select(selectClause).ToList();

            if (anonymousData is null || anonymousData.Count == 0)
                return Ok(new { items = Array.Empty<TEntity>() });

            var result = new List<IDictionary<string, object>>();
            foreach (var anonymousRow in anonymousData)
            {
                var convertedRow = new ExpandoObject() as IDictionary<string, object>;
                foreach (var propName in selectColumnsName)
                {
                    var propertyValue = ControllerQueryHelper.GetFieldValue<object>(anonymousRow, propName);
                    if (propertyValue is not null)
                        convertedRow.Add(propName.ToLower(), propertyValue ?? throw new Exception("property not found"));
                }
                result.Add(convertedRow);
            }

            IEnumerable<IDictionary<string, object>> resultRange = result;
            if (options.Skip is not null)
                resultRange = resultRange.Skip(options.Skip.Value);

            if (options.Top is not null)
                resultRange = resultRange.Take(options.Top.Value);

            if (options.Count is not null)
            {
                var jsonCountRes = JsonSerializer.Deserialize<object>(JsonSerializer.Serialize(resultRange, _jsonSettings));

                return Ok(new { items = jsonCountRes, result.Count });
            }

            var jsonRes = JsonSerializer.Deserialize<object>(JsonSerializer.Serialize(resultRange, _jsonSettings));


            return Ok(new { items = jsonRes });
        }

        if (options.Count is not null)
        {
            var queryResultCount = query.ToList();

            IEnumerable<TEntity> resultRange = queryResultCount;
            if (options.Skip is not null)
                resultRange = resultRange.Skip(options.Skip.Value);
            if (options.Top is not null)
                resultRange = resultRange.Take(options.Top.Value);

            var jsonFullCountRes = JsonSerializer.Deserialize<object>(JsonSerializer.Serialize(resultRange, _jsonSettings));

            return Ok(new { items = jsonFullCountRes, queryResultCount.Count });
        }

        if (options.Skip is not null)
            query = query.Skip(options.Skip.Value);

        if (options.Top is not null)
            query = query.Take(options.Top.Value);

        var queryResult = query.ToList();

        if (queryResult is null || queryResult.Count == 0)
            return Ok(new { items = Array.Empty<TEntity>() });

        var jsonFullRes = JsonSerializer.Deserialize<object>(JsonSerializer.Serialize(queryResult, _jsonSettings));

        return Ok(new { items = jsonFullRes });
    }

    [Route("GetById/{id}")]
    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public virtual async Task<ActionResult<TEntity>> GetById(int id)
    {
        var item = await _dataService.GetById(id);
        if (item is null)
            return NotFound();
        else
            return item;
    }

    [Route("Insert")]
    [HttpPost]
    [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public virtual async Task<ActionResult> Insert(TEntity? item)
    {
        if (item == null)
            return BadRequest();

        try
        {
            var res = await _dataService.Insert(item);

            return Ok(res);
        }
        catch
        {
            return BadRequest();
        }
    }

    [Route("Update")]
    [HttpPut]
    [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public virtual IActionResult Update(TEntity? item)
    {
        if (item == null)
            return BadRequest();

        int updateCount = _dataService.Update(item);

        if (updateCount > 0)
            return NoContent();
        else
            return BadRequest();
    }

    [Route("DeleteById/{id}")]
    [HttpDelete]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public virtual async Task<IActionResult> DeleteById(int id)
    {
        try 
        {
            var isDeleted = await _dataService.Delete(id);

            return Ok(isDeleted);
        }
        catch 
        {
            return Ok(false);
        }
    }

    [Route("Delete")]
    [HttpDelete]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public virtual IActionResult Delete(TEntity item)
    {
        var isDeleted = _dataService.Delete(item);

        if (!isDeleted)
            return NotFound(isDeleted);
        else
            return Ok(isDeleted);
    }
}
