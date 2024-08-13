# DemoResturantApplication

## Project Overview

DemoResturantApplication is a microservices-based restaurant management system that demonstrates modern software architecture and best practices. It leverages various technologies to create a scalable, maintainable, and efficient application.

## Key Features

- Microservices architecture for modularity and scalability
- API Gateway for centralized request handling and routing
- DAPR (Distributed Application Runtime) integration for microservices communication
- SQL Server database for data persistence
- Docker containerization for consistent deployment
- OAuth-based authentication for secure access

## Architecture

The application consists of the following microservices:

1. **Front Microservice**: Handles menu-related operations and customer management
2. **Orders Microservice**: Manages order processing and tracking
3. **Application Gateway**: Acts as an API Gateway, routing requests to appropriate microservices

## Technologies and Packages Used

### .NET Core

The application is built using .NET Core, providing cross-platform compatibility and high performance.

### Entity Framework Core

Used for object-relational mapping (ORM), simplifying database operations and providing a robust data access layer.

### DAPR (Distributed Application Runtime)

DAPR is used to abstract away the complexities of microservices communication, providing:

- Service-to-service invocation
- State management
- Pub/sub messaging
- Secrets management

### YARP (Yet Another Reverse Proxy)

YARP is used in the Application Gateway for efficient request routing and load balancing between microservices.

### JWT (JSON Web Tokens)

Used for secure authentication and authorization across the microservices.

### OData (Open Data Protocol)

Implemented to provide flexible querying capabilities for clients, allowing for filtering, sorting, and pagination of data.

### Docker

Used for containerization, ensuring consistent deployment across different environments.

### SQL Server

Chosen as the relational database management system for its robustness and compatibility with Entity Framework Core.

## Docker Compose Orchestration

The `docker-compose.yml` file orchestrates the deployment of all services:

1. **Front Microservice**:
   - Built from `MenuMicroservice/Dockerfile`
   - Paired with a DAPR sidecar container for microservices communication

2. **Application Gateway**:
   - Built from `ApplicationGateway/Dockerfile`
   - Exposed on port 14289
   - Paired with a DAPR sidecar container

3. **Orders Microservice**:
   - Built from `OrdersMicroservice/Dockerfile`
   - Paired with a DAPR sidecar container

4. **SQL Server**:
   - Uses the official Microsoft SQL Server 2022 image
   - Exposed on port 1435
   - Data persisted using a named volume

All services are connected to a custom Docker network called `my-dapr-network` for isolated communication.

## Getting Started

1. Ensure you have Docker and Docker Compose installed on your system.
2. Clone this repository.
3. Navigate to the project root directory.
4. Run `docker-compose up --build` to start all services.
5. Connect to the SQL Db at localhost,1435 user: SA password: YourStrong@Passw0rd and execute the DB creation script inside ApplicationGateway/DbCreationScript.txt to initialize the ResturantDB
6. Access the application via the Application Gateway at `http://localhost:14289`.

## Security

- JWT-based authentication is implemented for secure access to the microservices.
- Secrets are managed using DAPR's secret store component, keeping sensitive information separate from the application code.
- Note: a test JWT is available to use inside ApplicationGateway/AuthCredentials.txt

## Database

The SQL Server database is initialized with sample data for customers, menu items, and orders. The schema and initial data can be found in the `DbCreationScript.txt` file.

# DemoResturantApplication

[... Previous content remains the same ...]

## API Endpoints

Every controller inherit from the GenericController that provides a set of standard CRUD (Create, Read, Update, Delete) operations for each entity type plus every additional specific http methods. 
Below are the available endpoints:

### 1. Get (Read)
- **Endpoint**: `GET /{controller}/Get`
- **Description**: Retrieves a list of entities, supporting OData query options.
- **OData Options**: Supports $select, $expand, $filter, $orderby, $top, $skip, and $count.

### 2. GetById (Read)
- **Endpoint**: `GET /{controller}/GetById/{id}`
- **Description**: Retrieves a single entity by its ID.

### 3. Insert (Create)
- **Endpoint**: `POST /{controller}/Insert`
- **Description**: Creates a new entity.

### 4. Update (Update)
- **Endpoint**: `PUT /{controller}/Update`
- **Description**: Updates an existing entity.

### 5. DeleteById (Delete)
- **Endpoint**: `DELETE /{controller}/DeleteById/{id}`
- **Description**: Deletes an entity by its ID.

### 6. Delete (Delete)
- **Endpoint**: `DELETE /{controller}/Delete`
- **Description**: Deletes an entity based on the provided entity object.

## Using the Get Method with OData Options

The Get method in the GenericController supports powerful querying capabilities through OData options. Here's how to use various OData features:

### Counting Results
To get the total count of items along with the results:
```
GET /{controller}/Get?count=true
```

### Selecting Specific Fields
To retrieve only specific fields:
```
GET /{controller}/Get?select=name,price
```

### Expanding Related Entities
To include related entities in the response:
```
GET /{controller}/Get?expand=category
```

### Filtering Results
To filter the results based on certain conditions:
```
GET /{controller}/Get?filter=id eq 10
```

### Ordering Results
To order the results:
```
GET /{controller}/Get?orderby=name desc
```

### Pagination
To implement pagination:
```
GET /{controller}/Get?$top=10&$skip=20
```

### Combining Options
You can combine multiple OData options in a single request:
```
GET /{controller}/Get?$select=name,price&$filter=price gt 10&$orderby=name&$top=5&$skip=5&$count=true
```

This request will:
1. Select only the name and price fields
2. Filter for items with a price greater than 10
3. Order the results by name
4. Skip the first 5 results and take the next 5
5. Include the total count in the response

## Response Format

The Get method returns data in the following format:
```json
{
  "items": [
    // Array of entities or selected fields
  ],
  "count": 100  // Total count (if $count=true was specified)
}
```
