# Car Rent Service Web API

Welcome to the Car Rent Service Web API! This repository houses a web API application designed to facilitate car rentals. Users can place car rental orders specifying their desired rental period and rate the cars they have rented. This README provides an overview of the application's structure and how to get started.

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

The Car Rent Service Web API application offers the following core functionalities:

1. **Car Rental Orders**: Users can make car rental orders by specifying the start and end date-time for the rental period.

2. **Car Rating**: Users can rate the cars they have rented, providing feedback on their rental experience.

## Application Structure

The application is organized into several layers to ensure maintainability and separation of concerns:

- **CarRent.Api**: This API layer handles incoming HTTP requests, routing them to the appropriate application layer, and returning responses to clients.

- **CarRent.Application**: The application layer contains the business logic responsible for processing car rental orders and car ratings.

- **CarRent.Contracts**: The contracts layer defines request and response contracts, specifying the data structures exchanged between the API and the application layer.

- **Identity.Api**: This component serves as an internal JWT generator for authentication and authorization purposes.

- **CarRent.Application.Tests.Unit**: This unit test project contains tests for the `CarRent.Application` services to ensure the correctness of the business logic.

## Authentication and Authorization

The application uses JWT bearer authentication for securing its endpoints. Additionally, it implements two authorization policies:

- `Admin`: Users with admin privileges are authorized to perform specific actions.
- `TrustedMember`: Trusted members are authorized for specific actions.

Claims are assigned to users based on their roles, granting access to particular parts of the application.

## Database

The application relies on a PostgreSQL database to store car rental and user data. Dapper is used for object-relational mapping (ORM), enabling efficient interaction with the database.

## Testing

For testing purposes, the application leverages the following libraries:

- **xUnit**: This is the primary testing framework used for unit testing.
- **FluentAssertions**: It provides an expressive and fluent assertion syntax for test validations.
- **nSubstitute**: This library is used for mocking dependencies in unit tests, facilitating isolated testing of components.

## Getting Started

To run this application locally or in your own environment, follow these steps:

1. Clone this repository to your local machine.

2. Configure your PostgreSQL database connection string in the application settings.

3. Build and run the application.

4. Use a tool like Postman or a web browser to interact with the API endpoints.

## Postman Collection

To test the APIs, a Postman collection import file can be found in the 'Helpers' folder. Import this collection into Postman to easily make API requests with authentication enabled.

## Docker Compose

In the 'Helpers' folder, you'll also find a Docker Compose file that allows you to set up and run the PostgreSQL database needed for the application.

Enjoy using the Car Rent Service Web API! If you have any questions or encounter any issues, please refer to the documentation or feel free to reach out to us.
