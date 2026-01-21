# Gemini Chatbot

A chatbot application with a .NET backend and React frontend that uses Google's Gemini AI. Features user authentication for Amzur employees and persistent chat history stored in PostgreSQL.

## Project Structure

```
GeminiChatbot/
├── backend/         # .NET 8 Web API
│   ├── Controllers/
│   │   ├── AuthController.cs    # Authentication endpoints
│   │   └── ChatController.cs    # Chat and session endpoints
│   ├── Data/
│   │   └── ApplicationDbContext.cs
│   ├── Models/
│   │   ├── Entities/            # Database entities
│   │   └── ChatModels.cs
│   ├── Services/
│   │   ├── AuthService.cs       # JWT authentication
│   │   ├── ChatHistoryService.cs # Chat persistence
│   │   └── GeminiService.cs     # Gemini AI integration
│   └── appsettings.json
└── frontend/        # React CRA application
    └── src/
        ├── App.js
        ├── Login.js             # Login/Register component
        └── *.css
```

## Features

- **Amzur Employee Authentication**: Only users with @amzur.com email addresses can register
- **JWT Token-based Security**: Secure API endpoints with JSON Web Tokens
- **Persistent Chat History**: All conversations are stored in PostgreSQL
- **Multiple Chat Sessions**: Create, view, and delete chat sessions
- **Real-time Chat**: Communicate with Google's Gemini AI

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v18 or later)
- [PostgreSQL](https://www.postgresql.org/download/) (v14 or later)
- A free Gemini API key from [Google AI Studio](https://aistudio.google.com/app/apikey)

## Setup

### 1. Set Up PostgreSQL Database

1. Install PostgreSQL if not already installed
2. Create a database named `gemini_chatbot`:

```sql
CREATE DATABASE gemini_chatbot;
```

3. Update the connection string in `backend/appsettings.json` if needed:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=gemini_chatbot;Username=postgres;Password=postgres"
  }
}
```

### 2. Get a Gemini API Key

1. Go to [Google AI Studio](https://aistudio.google.com/app/apikey)
2. Click "Create API Key"
3. Copy the API key

### 3. Configure the Backend

1. Open `backend/appsettings.json`
2. Update the Gemini API key:

```json
{
  "Gemini": {
    "ApiKey": "your-actual-api-key-here"
  }
}
```

### 4. Run the Backend

```bash
cd backend
dotnet run --launch-profile https
```

The API will automatically apply database migrations on startup.

The API will be available at:
- https://localhost:7001 (HTTPS)
- http://localhost:5000 (HTTP)

Swagger UI: https://localhost:7001/swagger

### 5. Run the Frontend

In a new terminal:

```bash
cd frontend
npm install
npm start
```

The React app will be available at: http://localhost:3000

## Usage

1. Make sure PostgreSQL, backend, and frontend are running
2. Open http://localhost:3000 in your browser
3. Register with your @amzur.com email address
4. Login with your credentials
5. Start chatting! Your conversations are automatically saved
6. Use the sidebar to view previous chats or start a new conversation

## API Endpoints

### Authentication

#### POST /api/auth/register
Register a new Amzur employee.

**Request Body:**
```json
{
  "name": "John Doe",
  "email": "john.doe@amzur.com",
  "password": "yourpassword"
}
```

#### POST /api/auth/login
Login with existing credentials.

**Request Body:**
```json
{
  "email": "john.doe@amzur.com",
  "password": "yourpassword"
}
```

#### GET /api/auth/validate
Validate the current JWT token.

### Chat (Requires Authentication)

#### POST /api/chat
Send a message to the chatbot.

**Headers:**
```
Authorization: Bearer <your-jwt-token>
```

**Request Body:**
```json
{
  "message": "Hello, how are you?",
  "sessionId": 1,
  "history": []
}
```

#### GET /api/chat/sessions
Get all chat sessions for the current user.

#### GET /api/chat/sessions/{sessionId}/messages
Get all messages for a specific session.

#### POST /api/chat/sessions
Create a new chat session.

#### DELETE /api/chat/sessions/{sessionId}
Delete a chat session.
```

### GET /api/chat/health

Health check endpoint.

## Troubleshooting

### CORS Issues
The backend is configured to accept requests from http://localhost:3000. If you change the frontend port, update the CORS policy in `backend/Program.cs`.

### SSL Certificate Issues
If you see SSL certificate warnings, run:
```bash
dotnet dev-certs https --trust
```

### Connection Refused
Make sure the backend is running on https://localhost:7001 before using the frontend.

## Technologies Used

- **Backend**: .NET 8 Web API, HttpClient
- **Frontend**: React 18, Create React App
- **AI**: Google Gemini 1.5 Flash
