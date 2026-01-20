# Gemini Chatbot

A simple chatbot application with a .NET backend and React frontend that uses Google's Gemini AI.

## Project Structure

```
GeminiChatbot/
├── backend/         # .NET 8 Web API
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   └── appsettings.json
└── frontend/        # React CRA application
    └── src/
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v18 or later)
- A free Gemini API key from [Google AI Studio](https://aistudio.google.com/app/apikey)

## Setup

### 1. Get a Gemini API Key

1. Go to [Google AI Studio](https://aistudio.google.com/app/apikey)
2. Click "Create API Key"
3. Copy the API key

### 2. Configure the Backend

1. Open `backend/appsettings.json`
2. Replace `YOUR_GEMINI_API_KEY_HERE` with your actual Gemini API key:

```json
{
  "Gemini": {
    "ApiKey": "your-actual-api-key-here"
  }
}
```

### 3. Run the Backend

```bash
cd backend
dotnet run --launch-profile https
```

The API will be available at:
- https://localhost:7001 (HTTPS)
- http://localhost:5000 (HTTP)

Swagger UI: https://localhost:7001/swagger

### 4. Run the Frontend

In a new terminal:

```bash
cd frontend
npm start
```

The React app will be available at: http://localhost:3000

## Usage

1. Make sure both backend and frontend are running
2. Open http://localhost:3000 in your browser
3. Type a message and click Send
4. The chatbot will respond using Gemini AI

## API Endpoints

### POST /api/chat

Send a message to the chatbot.

**Request Body:**
```json
{
  "message": "Hello, how are you?",
  "history": [
    { "role": "user", "content": "Previous message" },
    { "role": "assistant", "content": "Previous response" }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "response": "I'm doing well, thank you for asking!"
}
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
