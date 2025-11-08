export const Severity = {
  BLOCKER: 'S1 - Blocker',
  CRITICAL: 'S2 - Critical',
  MAJOR: 'S3 - Major',
  MINOR: 'S4 - Minor',
};

export const Priority = {
  CRITICAL: 'P1 - Critical',
  HIGH: 'P2 - High',
  MEDIUM: 'P3 - Medium',
  LOW: 'P4 - Low',
};

export const Environment = {
  PRODUCTION: 'Production',
  STAGING: 'Staging',
  QA: 'QA',
  DEVELOPMENT: 'Development',
  LOCAL: 'Local',
};

// Helper functions to get arrays of values for dropdowns
export const getSeverityOptions = () => Object.values(Severity);
export const getPriorityOptions = () => Object.values(Priority);
export const getEnvironmentOptions = () => Object.values(Environment);

// Helper functions to get the enum key from value
export const getSeverityKey = (value) => Object.keys(Severity).find(key => Severity[key] === value);
export const getPriorityKey = (value) => Object.keys(Priority).find(key => Priority[key] === value);
export const getEnvironmentKey = (value) => Object.keys(Environment).find(key => Environment[key] === value);