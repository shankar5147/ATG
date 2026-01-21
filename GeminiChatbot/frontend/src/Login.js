import React, { useState, useEffect, useCallback } from 'react';
import './Login.css';

const API_BASE_URL = 'http://localhost:5000';

function Login({ onLogin }) {
  const [isRegister, setIsRegister] = useState(false);
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [googleClientId, setGoogleClientId] = useState(null);

  const handleGoogleResponse = useCallback(async (response) => {
    setError('');
    setIsLoading(true);

    try {
      const result = await fetch(`${API_BASE_URL}/api/auth/google`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ idToken: response.credential }),
      });

      const data = await result.json();

      if (data.success) {
        localStorage.setItem('token', data.token);
        localStorage.setItem('user', JSON.stringify(data.user));
        onLogin(data.user, data.token);
      } else {
        setError(data.error || 'Google login failed');
      }
    } catch (err) {
      console.error('Google auth error:', err);
      setError('Unable to connect to the server.');
    } finally {
      setIsLoading(false);
    }
  }, [onLogin]);

  useEffect(() => {
    // Fetch Google Client ID from backend
    const fetchGoogleClientId = async () => {
      try {
        const response = await fetch(`${API_BASE_URL}/api/auth/google/client-id`);
        if (response.ok) {
          const data = await response.json();
          setGoogleClientId(data.clientId);
        }
      } catch (err) {
        console.error('Failed to fetch Google Client ID:', err);
      }
    };

    fetchGoogleClientId();
  }, []);

  useEffect(() => {
    // Initialize Google Sign-In when client ID is available
    if (googleClientId && window.google) {
      window.google.accounts.id.initialize({
        client_id: googleClientId,
        callback: handleGoogleResponse,
      });

      window.google.accounts.id.renderButton(
        document.getElementById('google-signin-button'),
        { 
          theme: 'outline', 
          size: 'large', 
          width: '100%',
          text: 'signin_with',
          shape: 'rectangular'
        }
      );
    }
  }, [googleClientId, handleGoogleResponse]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      const endpoint = isRegister ? '/api/auth/register' : '/api/auth/login';
      const body = isRegister 
        ? { name, email, password }
        : { email, password };

      const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(body),
      });

      const data = await response.json();

      if (data.success) {
        localStorage.setItem('token', data.token);
        localStorage.setItem('user', JSON.stringify(data.user));
        onLogin(data.user, data.token);
      } else {
        setError(data.error || 'An error occurred');
      }
    } catch (err) {
      console.error('Auth error:', err);
      setError('Unable to connect to the server. Make sure the backend is running.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <div className="login-header">
          <h1>🤖 Gemini Chatbot</h1>
          <p>Powered by Google Gemini AI</p>
        </div>

        {/* Google Sign-In Button */}
        <div className="google-login-section">
          <div id="google-signin-button" className="google-btn-container"></div>
          {!googleClientId && (
            <button className="google-btn-fallback" disabled>
              <span className="google-icon">G</span>
              Sign in with Google
            </button>
          )}
        </div>

        <div className="divider">
          <span>or</span>
        </div>

        <div className="login-tabs">
          <button 
            className={`tab ${!isRegister ? 'active' : ''}`}
            onClick={() => { setIsRegister(false); setError(''); }}
          >
            Login
          </button>
          <button 
            className={`tab ${isRegister ? 'active' : ''}`}
            onClick={() => { setIsRegister(true); setError(''); }}
          >
            Register
          </button>
        </div>

        <form onSubmit={handleSubmit} className="login-form">
          {isRegister && (
            <div className="form-group">
              <label htmlFor="name">Name</label>
              <input
                type="text"
                id="name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Enter your name"
                required={isRegister}
              />
            </div>
          )}

          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              type="email"
              id="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="Enter your @amzur.com email"
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter your password"
              required
              minLength={6}
            />
          </div>

          {error && <div className="error-message">{error}</div>}

          <button type="submit" className="submit-btn" disabled={isLoading}>
            {isLoading ? 'Please wait...' : (isRegister ? 'Register' : 'Login')}
          </button>
        </form>

        <div className="login-footer">
          <p>Only Amzur employees (@amzur.com) can access this chatbot</p>
        </div>
      </div>
    </div>
  );
}

export default Login;
