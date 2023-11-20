
# Lifetimeline

**Lifetimeline** is a demonstration of a distributed .NET microservices application, using Docker for orchestration and featuring monitoring and distributed tracing capabilities. This project exemplifies the use of distributed tracing, monitoring for distributed systems, asynchronous design, and the fire-and-forget principle. It showcases how microservices like **BirthdayCollector** and **YearInHistoryService** interact through a user interface, with the entire flow managed by Docker Compose. Monitoring is implemented using Seq (accessible at localhost:5342) and Zipkin (localhost:9411).

The application allows users to enter their birthdate to calculate their current age and fetch historical events from their birth year through an API gateway.

![.NET](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![Microservices](https://img.shields.io/badge/Microservices-0078D6?style=for-the-badge&logo=microservices&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![HTML5](https://img.shields.io/badge/HTML5-E34F26?style=for-the-badge&logo=html5&logoColor=white)
![JavaScript](https://img.shields.io/badge/JavaScript-F7DF1E?style=for-the-badge&logo=javascript&logoColor=black)
![CSS3](https://img.shields.io/badge/CSS3-1572B6?style=for-the-badge&logo=css3&logoColor=white)

## Key Features

- Asynchronous communication between the `BirthdayCollector` and `YearInHistory` services.
- Utilization of circuit breakers and retry patterns via Polly for fault isolation and resilience.
- Distributed tracing and monitoring using OpenTelemetry, Zipkin, and Seq.
- UI service in MVC architecture acts as the domain that communicates with the `BirthdayCollector` service.

## Components

- **BirthdayCollector**: A microservice that calculates age from the birthdate and fetches historical events.
- **YearInHistoryService**: Handles fetching historical events for a specific year.
- **UI Service**: The front-end service providing the user interface for interaction.
- **Monitoring**: Implements logging and tracing across services.

## Technologies

- **.NET**: For building microservices and the UI.
- **Docker**: For containerization and orchestration.
- **Polly**: For implementing resilience patterns.
- **OpenTelemetry**: For distributed tracing.
- **Seq and Zipkin**: For monitoring and tracing visualization.

## Installation and Setup

To install and run the application:

1. Install dependencies for each service:
   - BirthdayCollector: Newtonsoft.json
   - YearInHistory: Polly, Polly.Extensions.Http
   - Monitoring: OpenTelemetry packages, Serilog
   - UI: Polly, Microsoft.Extensions.Http.Polly, Newtonsoft.json
2. Run `docker compose up –build web-ui` to start the application.
3. Access
   - **UI: localhost:8000**
   - **Seq: localhost:5342**
   - **Zipkin: localhost:9411**

## Contributing

Contributions are welcome. Special thanks to Wehba Korouni for the cooperation. This project is maintained by Said Mansour.


## Contact

For any inquiries or contributions, please open an issue on the project's GitHub page.

