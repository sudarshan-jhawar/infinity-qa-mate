/**
 * API Configuration
 * These values can be overridden using environment variables:
 * - REACT_APP_API_BASE_URL: Base URL for API endpoints
 * - REACT_APP_API_TIMEOUT: Default timeout for API requests in milliseconds
 */

export const API_CONFIG = {
  // Base URL for all API endpoints
  BASE_URL: process.env.REACT_APP_API_BASE_URL || 'http://localhost:3001/api',
  
  // Default request timeout (in milliseconds)
  TIMEOUT: parseInt(process.env.REACT_APP_API_TIMEOUT || '30000', 10),
  
  // API endpoints
  ENDPOINTS: {
    DEFECTS: {
      BASE: '/defects',
      CHECK_TITLE: '/exists',
      ATTACHMENTS: '/attachments'
    }
  },
  
  // HTTP headers
  HEADERS: {
    'Content-Type': 'application/json',
    'Accept': 'application/json'
  }
};

/**
 * Constructs a full API URL from the base URL and endpoint
 * @param {string} endpoint - The API endpoint path
 * @returns {string} - The complete API URL
 */
export const getApiUrl = (endpoint) => {
  return `${API_CONFIG.BASE_URL}${endpoint}`;
};

/**
 * Default fetch options for API requests
 */
export const DEFAULT_FETCH_OPTIONS = {
  headers: API_CONFIG.HEADERS,
  timeout: API_CONFIG.TIMEOUT
};