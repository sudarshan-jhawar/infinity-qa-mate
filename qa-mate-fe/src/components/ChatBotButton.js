import React, { useState, useEffect, useCallback } from 'react';
import './ChatBotButton.css';

const initialSuggestions = [
  'How to create a defect?',
  'Required fields?',
  'Adding attachments',
  'Priority levels',
  'Severity guidelines'
];

const botResponses = {
  'how to create a defect': 
    'To create a defect, fill in the following required fields:\n1. Title\n2. Description\n3. Steps to Reproduce\n4. Severity\n5. Priority\n\nThen click the "Submit" button at the bottom of the form.',
  
  'required fields':
    'The required fields for a defect are:\n- Title (Brief description)\n- Description (Detailed issue explanation)\n- Steps to Reproduce\n- Severity (Impact level)\n- Priority (Urgency level)',
  
  'adding attachments':
    'To add attachments:\n1. Click the "Choose Files" button\n2. Select one or multiple files\n3. Your files will be shown with previews\n4. You can remove them by clicking the "X" button',
  
  'priority levels':
    'Priority levels indicate urgency:\nP1 - Critical (Immediate action required)\nP2 - High (Urgent but not critical)\nP3 - Medium (Important but not urgent)\nP4 - Low (Minor importance)',
  
  'severity guidelines':
    'Severity levels indicate impact:\nS1 - Blocker (System unusable)\nS2 - Critical (Major feature broken)\nS3 - Major (Significant impact)\nS4 - Minor (Minor impact)'
};

const getBotResponse = (message) => {
  const normalizedMessage = message.toLowerCase();
  for (const [key, response] of Object.entries(botResponses)) {
    if (normalizedMessage.includes(key)) {
      return response;
    }
  }
  return "I'm here to help with defect creation. You can ask about:\n- How to create a defect\n- Required fields\n- Adding attachments\n- Priority levels\n- Severity guidelines";
};

export default function ChatBotButton() {
  const [isOpen, setIsOpen] = useState(false);
  const [messages, setMessages] = useState([
    { type: 'bot', text: 'Hi! How can I help you with defect creation?' }
  ]);
  const [inputText, setInputText] = useState('');

  const toggleChat = () => setIsOpen(!isOpen);

  const handleClickOutside = useCallback((event) => {
    if (!event.target.closest('.chatbot-container')) {
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

  const handleSendMessage = (text) => {
    if (!text.trim()) return;

    setMessages(prev => [...prev, 
      { type: 'user', text: text },
      { type: 'bot', text: getBotResponse(text) }
    ]);
    setInputText('');
  };

  const handleKeyPress = (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage(inputText);
    }
  };

  const handleSuggestionClick = (suggestion) => {
    handleSendMessage(suggestion);
  };

  return (
    <div className="chatbot-container">
      {isOpen && (
        <div className="chat-window">
          <div className="chat-header">
            <span>Defect Assistant</span>
            <button onClick={toggleChat}>×</button>
          </div>
          <div className="chat-messages">
            {messages.map((msg, index) => (
              <div key={index} className={`message ${msg.type}-message`}>
                {msg.text}
              </div>
            ))}
          </div>
          <div className="suggestions">
            {initialSuggestions.map((suggestion, index) => (
              <button
                key={index}
                className="suggestion-chip"
                onClick={() => handleSuggestionClick(suggestion)}
              >
                {suggestion}
              </button>
            ))}
          </div>
          <div className="chat-input">
            <input
              type="text"
              value={inputText}
              onChange={(e) => setInputText(e.target.value)}
              onKeyPress={handleKeyPress}
              placeholder="Type your question..."
            />
            <button onClick={() => handleSendMessage(inputText)}>Send</button>
          </div>
        </div>
      )}
      <button 
        className={`chat-button ${isOpen ? 'active' : ''}`} 
        onClick={toggleChat}
        aria-label="Chat with defect assistant"
      >
        <span role="img" aria-hidden="true">💬</span>
      </button>
    </div>
  );
}