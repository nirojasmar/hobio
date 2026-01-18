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

*   **Framework**: .NET 10 (Preview/Latest)
*   **Language**: C#
*   **Database/Cache**: Redis
*   **Containerization**: Docker
*   **Cloud Provider**: Google Cloud Platform (GCP)

## 📂 Project Structure

*   **`api/`**: The ASP.NET Core Web API project.
*   **`worker/`**: The Worker Service project for background processing.
*   **`shared/`**: Shared libraries and models used across services.

## 🚀 Getting Started

### Prerequisites

*   [.NET 10 SDK](https://dotnet.microsoft.com/download)
*   [Docker](https://www.docker.com/products/docker-desktop)
*   [Redis](https://redis.io/) (Local instance or via Docker)

### Running Locally

1.  **Start Redis**:
    Ensure you have a Redis instance running locally (default port 6379).
    ```bash
    docker run -d -p 6379:6379 redis
    ```

2.  **Run the API**:
    Navigate to the `api` directory and start the service.
    ```bash
    cd api
    dotnet run
    ```
    The API will be available at `http://localhost:5000` (or similar, check console output).

3.  **Run the Worker**:
    Open a new terminal, navigate to the `worker` directory and start the service.
    ```bash
    cd worker
    dotnet run
    ```

## 🗺 Roadmap

*   [ ] Implement Redis queue integration.
*   [ ] Integrate Steam API.
*   [ ] Integrate Last.fm API.
*   [ ] Implement PDF generation logic.
*   [ ] Create Docker Compose setup for easy local orchestration.
*   [ ] Deploy to GCP (Google Cloud Run/GKE).
*   [ ] Develop a Frontend UI (separate project).

## 📄 License

[MIT License](LICENSE)
