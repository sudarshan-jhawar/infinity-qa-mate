import React from 'react';
import './DefectForm.css';

export default function ConfirmDialog({ open, title = 'Confirm', message, onConfirm, onCancel }) {
  if (!open) return null;
  return (
    <div className="cf-overlay" data-testid="confirm-dialog">
      <div className="cf-dialog">
        <div className="cf-header">
          <strong>{title}</strong>
        </div>
        <div className="cf-body">
          <p>{message}</p>
        </div>
        <div className="cf-actions">
          <button className="btn ghost" data-testid="confirm-cancel" onClick={onCancel}>Cancel</button>
          <button className="btn primary" data-testid="confirm-ok" onClick={onConfirm}>OK</button>
        </div>
      </div>
    </div>
  );
}
