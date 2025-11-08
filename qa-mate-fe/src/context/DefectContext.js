import React, { createContext, useContext, useState } from 'react';
import { Severity, Priority, Environment } from '../constants/defectEnums';

const DefectContext = createContext(null);

export function DefectProvider({ children }) {
  const [defect, setDefect] = useState({
    title: '',
    description: '',
    steps: '',
    severity: Severity.MAJOR,
    priority: Priority.HIGH,
    environment: Environment.QA,
    // attachments will be objects: { name, type, url, file }
    attachments: [],
    logsAttached: false,
  });

  const [status, setStatus] = useState({ submitting: false, success: null, error: null });

  const updateField = (field, value) => {
    setDefect(prev => ({ ...prev, [field]: value }));
  };

  const addAttachment = (file) => {
    // create a preview URL for images/videos
    const url = file && typeof file === 'object' && file instanceof File ? URL.createObjectURL(file) : null;
    const item = {
      name: file.name || String(file),
      type: file.type || (file.name && file.name.split('.').pop()) || 'unknown',
      url,
      file: file instanceof File ? file : null,
    };
    setDefect(prev => ({ ...prev, attachments: [...prev.attachments, item] }));
  };

  const removeAttachment = (index) => {
    setDefect(prev => {
      const toRemove = prev.attachments[index];
      if (toRemove && toRemove.url) {
        try { URL.revokeObjectURL(toRemove.url); } catch (e) { /* ignore */ }
      }
      return { ...prev, attachments: prev.attachments.filter((_, i) => i !== index) };
    });
  };

  const revokeAllAttachments = () => {
    setDefect(prev => {
      prev.attachments.forEach(a => { if (a && a.url) { try { URL.revokeObjectURL(a.url); } catch (e) {} } });
      return { ...prev, attachments: [] };
    });
  };

  const resetDefect = () => {
    // revoke existing object URLs
    defect.attachments.forEach(a => { if (a && a.url) { try { URL.revokeObjectURL(a.url); } catch (e) {} } });
    setDefect({ 
      title: '', 
      description: '', 
      steps: '', 
      severity: Severity.MAJOR, 
      priority: Priority.HIGH, 
      environment: Environment.QA, 
      attachments: [], 
      logsAttached: false 
    });
  };

  return (
    <DefectContext.Provider value={{ defect, updateField, addAttachment, removeAttachment, resetDefect, revokeAllAttachments, status, setStatus }}>
      {children}
    </DefectContext.Provider>
  );
}

export function useDefectContext() {
  const ctx = useContext(DefectContext);
  if (!ctx) throw new Error('useDefectContext must be used within a DefectProvider');
  return ctx;
}

export default DefectContext;
