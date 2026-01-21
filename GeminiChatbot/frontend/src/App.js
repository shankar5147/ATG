import React, { useState, useRef, useEffect } from 'react';
import Login from './Login';
import './App.css';

const API_BASE_URL = 'http://localhost:5000';

function App() {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(null);
  const [messages, setMessages] = useState([]);
  const [inputMessage, setInputMessage] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [sessions, setSessions] = useState([]);
  const [currentSessionId, setCurrentSessionId] = useState(null);
  const [showSidebar, setShowSidebar] = useState(true);
  const messagesEndRef = useRef(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  // Check for existing token on mount
  useEffect(() => {
    const savedToken = localStorage.getItem('token');
    const savedUser = localStorage.getItem('user');
    
    if (savedToken && savedUser) {
      validateToken(savedToken, JSON.parse(savedUser));
    }
  }, []);

  // Load sessions when user logs in
  useEffect(() => {
    if (user && token) {
      loadSessions();
    }
  }, [user, token]);

  const validateToken = async (savedToken, savedUser) => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/validate`, {
        headers: {
          'Authorization': `Bearer ${savedToken}`
        }
      });

      if (response.ok) {
        setToken(savedToken);
        setUser(savedUser);
      } else {
        // Token invalid, clear storage
        localStorage.removeItem('token');
        localStorage.removeItem('user');
      }
    } catch (error) {
      console.error('Token validation error:', error);
    }
  };

  const loadSessions = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/chat/sessions`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const data = await response.json();
        setSessions(data);
      }
    } catch (error) {
      console.error('Error loading sessions:', error);
    }
  };

  const loadSessionMessages = async (sessionId) => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/chat/sessions/${sessionId}/messages`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const data = await response.json();
        setMessages(data.map(m => ({
          role: m.role,
          content: m.content
        })));
        setCurrentSessionId(sessionId);
      }
    } catch (error) {
      console.error('Error loading messages:', error);
    }
  };

  const createNewSession = async () => {
    setMessages([]);
    setCurrentSessionId(null);
  };

  const deleteSession = async (sessionId) => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/chat/sessions/${sessionId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        setSessions(sessions.filter(s => s.id !== sessionId));
        if (currentSessionId === sessionId) {
          setMessages([]);
          setCurrentSessionId(null);
        }
      }
    } catch (error) {
      console.error('Error deleting session:', error);
    }
  };

  const handleLogin = (userData, userToken) => {
    setUser(userData);
    setToken(userToken);
  };

  const handleLogout = () => {
    setUser(null);
    setToken(null);
    setMessages([]);
    setSessions([]);
    setCurrentSessionId(null);
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  };

  const sendMessage = async (e) => {
    e.preventDefault();
    
    if (!inputMessage.trim() || isLoading) return;

    const userMessage = {
      role: 'user',
      content: inputMessage
    };

    setMessages(prev => [...prev, userMessage]);
    setInputMessage('');
    setIsLoading(true);

    try {
      const response = await fetch(`${API_BASE_URL}/api/chat`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
          message: inputMessage,
          history: messages,
          sessionId: currentSessionId
        }),
      });

      const data = await response.json();

      if (data.success) {
        const assistantMessage = {
          role: 'assistant',
          content: data.response
        };
        setMessages(prev => [...prev, assistantMessage]);
        
        // Update session ID if this was a new session
        if (!currentSessionId && data.sessionId) {
          setCurrentSessionId(data.sessionId);
          loadSessions(); // Refresh sessions list
        }
      } else {
        const errorMessage = {
          role: 'assistant',
          content: `Error: ${data.error || 'Something went wrong. Please try again.'}`
        };
        setMessages(prev => [...prev, errorMessage]);
      }
    } catch (error) {
      console.error('Error sending message:', error);
      const errorMessage = {
        role: 'assistant',
        content: 'Error: Unable to connect to the server. Make sure the backend is running on http://localhost:5000'
      };
      setMessages(prev => [...prev, errorMessage]);
    } finally {
      setIsLoading(false);
    }
  };

  // Show login if not authenticated
  if (!user || !token) {
    return <Login onLogin={handleLogin} />;
  }

  return (
    <div className="app">
      {/* Sidebar for chat history */}
      <aside className={`sidebar ${showSidebar ? 'open' : ''}`}>
        <div className="sidebar-header">
          <h3>Chat History</h3>
          <button className="new-chat-btn" onClick={createNewSession}>
            + New Chat
          </button>
        </div>
        <div className="sessions-list">
          {sessions.length === 0 ? (
            <p className="no-sessions">No previous chats</p>
          ) : (
            sessions.map(session => (
              <div 
                key={session.id} 
                className={`session-item ${currentSessionId === session.id ? 'active' : ''}`}
                onClick={() => loadSessionMessages(session.id)}
              >
                <span className="session-title">{session.title}</span>
                <button 
                  className="delete-session-btn"
                  onClick={(e) => {
                    e.stopPropagation();
                    deleteSession(session.id);
                  }}
                >
                  🗑️
                </button>
              </div>
            ))
          )}
        </div>
        <div className="sidebar-footer">
          <div className="user-info">
            <span>👤 {user.name}</span>
          </div>
          <button className="logout-btn" onClick={handleLogout}>
            Logout
          </button>
        </div>
      </aside>

      {/* Main chat area */}
      <div className="main-content">
        <header className="app-header">
          <button className="toggle-sidebar" onClick={() => setShowSidebar(!showSidebar)}>
            ☰
          </button>
          <h1>🤖 Gemini Chatbot</h1>
          <p>Powered by Google Gemini AI</p>
        </header>

        <main className="chat-container">
          <div className="messages-container">
            {messages.length === 0 && (
              <div className="welcome-message">
                <h2>Welcome, {user.name}! 👋</h2>
                <p>Start a conversation with Gemini AI. Ask me anything!</p>
              </div>
            )}
            
            {messages.map((message, index) => (
              <div
                key={index}
                className={`message ${message.role === 'user' ? 'user-message' : 'assistant-message'}`}
              >
                <div className="message-avatar">
                  {message.role === 'user' ? '👤' : '🤖'}
                </div>
                <div className="message-content">
                  <div className="message-role">
                    {message.role === 'user' ? 'You' : 'Gemini'}
                  </div>
                  <div className="message-text">
                    {message.content}
                  </div>
                </div>
              </div>
            ))}
            
            {isLoading && (
              <div className="message assistant-message">
                <div className="message-avatar">🤖</div>
                <div className="message-content">
                  <div className="message-role">Gemini</div>
                  <div className="message-text typing-indicator">
                    <span></span>
                    <span></span>
                    <span></span>
                  </div>
                </div>
              </div>
            )}
            
            <div ref={messagesEndRef} />
          </div>

          <form className="input-container" onSubmit={sendMessage}>
            <input
              type="text"
              value={inputMessage}
              onChange={(e) => setInputMessage(e.target.value)}
              placeholder="Type your message here..."
              disabled={isLoading}
            />
            <button type="submit" disabled={isLoading || !inputMessage.trim()}>
              {isLoading ? '...' : 'Send'}
            </button>
          </form>
        </main>
      </div>
    </div>
  );
}

export default App;
