# Web API Application Readme

Welcome to the documentation for our Web API application! This repository contains the codebase for a web API application that is structured into various layers and components to provide a robust and maintainable solution. Below, you will find an overview of the key components, technologies, and practices used in this project.

## Table of Contents

- [Project Structure](#project-structure)
- [Authentication and Authorization](#authentication-and-authorization)
- [Database Access](#database-access)
- [Validation](#validation)
- [Unit Testing](#unit-testing)
- [Testing the API](#testing-the-api)
- [Dependencies](#dependencies)

## Project Structure

The project is organized into several layers, each serving a specific purpose:

1. **Back.Api**: This layer represents the API endpoints and controllers responsible for handling incoming HTTP requests. It serves as the entry point for the application.

2. **Back.Application**: The Application layer contains the business logic of the application. It's responsible for processing data, applying rules, and orchestrating interactions between different components.

3. **Back.Contracts**: Contracts are used for defining request and response models. These contracts help ensure a clear and consistent communication interface between various layers of the application.

4. **Identity.Api**: This internal component is responsible for generating JSON Web Tokens (JWT) used for authentication and authorization purposes.

5. **Back.Application.Tests**: This project contains unit tests for the services and components within the `Back.Application` layer. We use xUnit and FluentAssertion libraries for writing and running tests, along with nSubstitute for mocking dependencies.

## Authentication and Authorization

- **JWT Bearer Authentication**: We use JSON Web Tokens (JWT) for authenticating and authorizing users. Requests to protected API endpoints must include a valid JWT token in the authorization header.

- **Authorization Claims**: Our application defines two main authorization claims:
    - `Admin`: Grants full administrative privileges.
    - `TrustedMember`: Provides access to specific trusted user features.

## Database Access

- **Dapper**: We use Dapper as an Object-Relational Mapping (ORM) library to map database objects to .NET objects. It simplifies database interactions and enhances performance.

## Validation

- **FluentValidation**: To ensure data integrity and validate models, we employ FluentValidation, a popular validation library in the .NET ecosystem. It helps maintain consistency and reliability in the application's data processing.

## Unit Testing

- **xUnit and FluentAssertion**: Unit tests are crucial for maintaining code quality. We utilize xUnit as our unit testing framework and FluentAssertion to write clear and expressive test assertions. This ensures that our code is reliable and behaves as expected.

- **nSubstitute**: For mocking dependencies during unit testing, we use nSubstitute to isolate and test individual components independently.

## Testing the API

You have two convenient options for testing the API:

1. **Swagger UI**: Our application includes Swagger UI, which provides an interactive interface for exploring and testing API endpoints. To access Swagger UI, run the application and navigate to the following URL in your browser:

   ```
   http://localhost:PORT/swagger
   ```

   Replace `PORT` with the actual port number your application is running on. Swagger UI allows you to explore API endpoints, and make requests.

2. **Postman**: We have provided a Postman collection export file that contains information about API endpoints for generating JWT tokens and making requests.

   - Postman Collection File: [helpers/postman_collection.json](helpers/postman_collection.json)

   This collection file simplifies the process of testing various API endpoints, including those that require authentication and authorization. You can import this collection into Postman and start testing the API immediately.

## Dependencies

Ensure you have the following dependencies installed and configured before running the application:

- .NET SDK (version 7.0.10)
- Visual Studio or Visual Studio Code (or your preferred IDE)
- SQL Server (or another compatible database system)
- Any additional dependencies listed in the project's `README` or `csproj` files.

For detailed setup and usage instructions, please refer to the project-specific documentation or README files in each component's directory.

Thank you for using our Web API application. If you have any questions, encounter issues, or would like to contribute, please feel free to reach out to the project maintainers. Happy coding!
