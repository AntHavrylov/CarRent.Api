# Car Rent Service Web API

Welcome to the Car Rent Service Web API repository! This web API application provides the functionality to rent cars for specified date and time ranges and allows users to rate the cars they have rented. This README will guide you through the application's structure and how to get started.

## Table of Contents

- [Application Overview](#application-overview)
- [Application Structure](#application-structure)
- [Authentication and Authorization](#authentication-and-authorization)
- [Database](#database)
- [Testing](#testing)
- [Getting Started](#getting-started)
- [Postman Collection](#postman-collection)
- [Docker Compose](#docker-compose)

## Application Overview

The Car Rent Service Web API offers the following key functionalities:

1. **Car Rental Orders**: Users can create car rental orders by specifying the desired rental period, from date-time to date-time.

2. **Car Ratings**: Users can rate the cars they have rented, providing valuable feedback about their rental experience.

## Application Structure

The application is structured into different layers to maintain organization and separation of concerns:

- **CarRent.Api**: This API layer handles incoming HTTP requests, routing them to the appropriate components and returning responses to clients.

- **CarRent.Application**: The application layer contains the core business logic responsible for processing car rental orders and handling car ratings.

- **CarRent.Contracts**: The contracts layer defines request and response contracts, specifying the data structures exchanged between the API and the application layer.

- **Identity.Api**: This component serves as an internal JWT generator, providing authentication and authorization capabilities.

- **CarRent.Application.Tests.Unit**: This unit test project contains tests for the `CarRent.Application` services and validators to ensure the correctness of the business logic and data validation.

- **CarRent.Api.Tests.Unit**: This unit test project focuses on testing the APIs and associated validators in the `CarRent.Api` project.

## Authentication and Authorization

The application uses JWT Bearer authentication for securing its endpoints. Additionally, it implements two authorization policies:

- `Admin`: Users with admin privileges are authorized to perform specific actions.
- `TrustedMember`: Trusted members are authorized for specific actions.

Users are granted claims corresponding to their roles, allowing access to specific parts of the application.

## Database

The application relies on a PostgreSQL database to store car rental and user data. Dapper is utilized for object-relational mapping (ORM), ensuring efficient interaction with the database.

## Testing

To maintain code quality and reliability, the application utilizes the following testing libraries:

- **xUnit**: This is the primary testing framework used for unit testing.
- **FluentAssertions**: It offers an expressive and fluent assertion syntax for test validations.
- **nSubstitute**: This library is employed for mocking dependencies in unit tests, facilitating isolated testing of components.

## Getting Started

To run this application locally or in your own environment, follow these steps:

1. Clone this repository to your local machine.

2. Configure your PostgreSQL database connection string in the application settings.

3. Build and run the application.

4. Utilize a tool like Postman, a web browser, or Swagger to interact with the API endpoints.

## Postman Collection

To facilitate the testing of APIs, a Postman collection import file can be found in the 'Helpers' folder. Import this collection into Postman to easily make API requests with authentication enabled.

## Docker Compose

In the 'Helpers' folder, you will also find a Docker Compose file that allows you to set up and run the PostgreSQL database required for the application.

Enjoy using the Car Rent Service Web API! Should you have any questions or encounter any issues, please refer to the documentation or feel free to reach out to us.
