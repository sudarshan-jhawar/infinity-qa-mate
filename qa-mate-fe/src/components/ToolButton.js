import React, { useState, useEffect, useCallback } from 'react';
import './ToolButton.css';

export default function ToolButton({ onAction }) {
  const [isOpen, setIsOpen] = useState(false);

  const toggleMenu = () => setIsOpen(!isOpen);

  const handleClickOutside = useCallback((event) => {
    if (!event.target.closest('.tool-button-container')) {
      setIsOpen(false);
    }
  }, []);

  useEffect(() => {
    if (isOpen) {
      document.addEventListener('click', handleClickOutside);
    }
    return () => {
      document.removeEventListener('click', handleClickOutside);
    };
  }, [isOpen, handleClickOutside]);

  const handleAction = (action) => {
    onAction(action);
    setIsOpen(false);
  };

  return (
    <div className="tool-button-container">
      {isOpen && (
        <div className="tool-menu">
          <button 
            className="tool-item" 
            onClick={() => handleAction('screenshot')}
          >
            <span role="img" aria-label="camera">📸</span>
            Screenshot
          </button>
          <button 
            className="tool-item" 
            onClick={() => handleAction('record')}
          >
            <span role="img" aria-label="video">🎥</span>
            Record Video
          </button>
          <button 
            className="tool-item" 
            onClick={() => handleAction('voice')}
          >
            <span role="img" aria-label="microphone">🎤</span>
            Voice Note
          </button>
          <button 
            className="tool-item" 
            onClick={() => handleAction('logs')}
          >
            <span role="img" aria-label="document">📄</span>
            System Logs
          </button>
        </div>
      )}
      <button 
        className={`tool-button ${isOpen ? 'active' : ''}`} 
        onClick={toggleMenu}
        aria-label="Tools menu"
        data-testid="tool-button"
      >
        <span role="img" aria-hidden="true">🔧</span>
      </button>
    </div>
  );
}