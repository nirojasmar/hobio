# Hobio

> Create beautiful PDF reports of your gaming and listening habits.

Hobio is a personal project designed to generate detailed PDF reports based on user statistics from **Steam** and **Last.fm**. It is built as a distributed system using .NET, designed to be containerized and hosted on Google Cloud Platform (GCP).

## 🏗 Architecture

The application follows a decoupled architecture/microservices pattern to handle report generation efficiently:

1.  **Hobio.API**: The entry point of the application. It receives User requests to generate reports and enqueues them into a Redis queue.
2.  **Redis**: Acts as the message broker/queue, holding report generation tasks.
3.  **Hobio.Worker**: A background worker service that dequeues tasks, processes the data (fetching from Steam/Last.fm integration), generates the PDF, and makes it available for download.

Future updates aim to support additional platforms beyond Steam and Last.fm.

## 🛠 Tech Stack

*   **Framework**: .NET 10
*   **Language**: C#
*   **Database/Cache**: RabbitMQ
*   **Containerization**: Docker (using Alpine-based images for efficiency)
*   **Orchestration**: Docker Compose
*   **Cloud Provider**: Google Cloud Platform (GCP)

## 📂 Project Structure

*   **`api/`**: The ASP.NET Core Web API project.
*   **`worker/`**: The Worker Service project for background processing. Includes `ttf-dejavu` for PDF font support.
*   **`shared/`**: Shared libraries and models used across services.

## 🚀 Getting Started

### Prerequisites

*   [.NET 10 SDK](https://dotnet.microsoft.com/download)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Running with Docker Compose (Recommended)

The easiest way to run the entire stack (API, Worker, and Redis) is using Docker Compose:

1.  **Start the stack**:
    ```bash
    docker-compose up --build
    ```
    This will:
    *   Build the `api` and `worker` images using their respective Dockerfiles.
    *   Start a `rabbitmq:3-alpine` container.
    *   Set up a dedicated network (`hobio_network`).
    *   Wait for RabbitMQ to be healthy before starting the services.

2.  **Access the API**:
    The API will be available at `http://localhost:8080`.

### Running Manually

If you prefer to run services individually:

1.  **Start RabbitMQ**:
    ```bash
    docker run -d -p 5672:5672 --name hobio_rabbitmq rabbitmq:3-alpine
    ```

2.  **Run the SDK projects**:
    You will need to set the `RABBITMQ_URL` environment variable if your RabbitMQ is not on `localhost:5672`.
    ```bash
    # Terminal 1 (API)
    cd api
    dotnet run

    # Terminal 2 (Worker)
    cd worker
    dotnet run
    ```

## 🗺 Roadmap

*   [x] Create Docker Compose setup for easy local orchestration.
*   [x] Implement RabbitMQ queue integration.
*   [ ] Integrate Steam API.
*   [ ] Integrate Last.fm API.
*   [x] Implement PDF generation logic.
*   [ ] Deploy to GCP (Google Cloud Run/GKE).
*   [ ] Develop a Frontend UI (separate project).

## 📄 License

[MIT License](LICENSE)
