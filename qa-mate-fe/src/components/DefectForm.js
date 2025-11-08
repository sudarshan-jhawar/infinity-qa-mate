import React, { useEffect, useState, useRef } from 'react';
import './DefectForm.css';
import { useDefectContext } from '../context/DefectContext';
import ConfirmDialog from './ConfirmDialog';
import { 
  getSeverityOptions, 
  getPriorityOptions, 
  getEnvironmentOptions,
  Severity,
  Priority,
  Environment 
} from '../constants/defectEnums';

function DefectForm() {
  const { defect, updateField, addAttachment, removeAttachment, resetDefect, status, setStatus } = useDefectContext();
  const [errors, setErrors] = useState({});
  const [isDragging, setIsDragging] = useState(false);
  const fileInputRef = useRef(null);

  useEffect(() => {
    if (status.success || status.error) {
      const t = setTimeout(() => setStatus({ submitting: false, success: null, error: null }), 3500);
      return () => clearTimeout(t);
    }
  }, [defect, status, setStatus]);

  const validate = () => {
    const e = {};
    if (!defect.title.trim()) e.title = 'Defect title is required';
    if (!defect.description.trim()) e.description = 'Description is required';
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  // placeholder for API integration
  const submitDefect = async (payload) => {
    setStatus({ submitting: true, success: null, error: null });
    try {
      // simulate network
      await new Promise((r) => setTimeout(r, 800));
      // TODO: replace with real API call
      setStatus({ submitting: false, success: 'Defect submitted successfully', error: null });
      return { ok: true };
    } catch (err) {
      setStatus({ submitting: false, success: null, error: err?.message || 'Submission failed' });
      return { ok: false };
    }
  };

  const onSubmit = async (e) => {
    e.preventDefault();
    if (!validate()) return;
    const res = await submitDefect(defect);
    if (res.ok) resetDefect();
  };

  const onFileChange = (ev) => {
    const files = ev.target.files;
    if (!files) return;
    Array.from(files).forEach(file => addAttachment(file));
    // reset input
    ev.target.value = null;
  };

  const onDragOver = (ev) => {
    ev.preventDefault();
    ev.stopPropagation();
    setIsDragging(true);
  };

  const onDragLeave = (ev) => {
    ev.preventDefault();
    ev.stopPropagation();
    setIsDragging(false);
  };

  const onDrop = (ev) => {
    ev.preventDefault();
    ev.stopPropagation();
    setIsDragging(false);
    const files = ev.dataTransfer && ev.dataTransfer.files;
    if (!files) return;
    Array.from(files).forEach(file => addAttachment(file));
  };

  const [confirm, setConfirm] = useState({ open: false, index: null, name: '' });

  const requestRemove = (index) => {
    const item = defect.attachments[index];
    setConfirm({ open: true, index, name: item?.name || 'this file' });
  };

  const handleConfirm = () => {
    if (confirm.index !== null) removeAttachment(confirm.index);
    setConfirm({ open: false, index: null, name: '' });
  };

  const handleCancel = () => setConfirm({ open: false, index: null, name: '' });

  return (
    <div className="defect-card">
      <div className="defect-header">
        <h3 className="defect-title">Log New Defect</h3>
      </div>

      <form className="defect-body" onSubmit={onSubmit}>
        <div className="left-column">
          <div className="field">
            <label htmlFor="title">Defect Title</label>
            <input id="title" type="text" value={defect.title} onChange={(e) => updateField('title', e.target.value)} />
            {errors.title && <div style={{ color: '#dc2626', fontSize: 13 }}>{errors.title}</div>}
          </div>

          <div className="field">
            <label htmlFor="description">Description</label>
            <textarea id="description" value={defect.description} onChange={(e) => updateField('description', e.target.value)} />
            {errors.description && <div style={{ color: '#dc2626', fontSize: 13 }}>{errors.description}</div>}
          </div>

          <div className="field">
            <label htmlFor="steps">Steps to Reproduce</label>
            <textarea id="steps" value={defect.steps} onChange={(e) => updateField('steps', e.target.value)} />
          </div>

          <div className="field evidence">
            <label>Evidence Capture</label>
            <div className="evidence-row">
              <div className="evidence-card">
                <div style={{ fontSize: 13, color: '#0b1320', marginBottom: 8 }}>Logs</div>
                <button type="button" className="btn ghost">View Logs</button>
              </div>

              <div className="evidence-card">
                <div style={{ fontSize: 13, color: '#0b1320', marginBottom: 8 }}>Screenshots</div>
                <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                  {defect.attachments.filter(a => a.type && a.type.startsWith && a.type.startsWith('image/')).length === 0 && (
                    <div style={{ fontSize: 12, color: '#475569' }}>Auto-attached</div>
                  )}
                  {defect.attachments.map((att, idx) => att.type && att.type.startsWith && att.type.startsWith('image/') ? (
                    <div key={idx} className="thumb" style={{ width: 100 }}>
                      <img data-testid={`img-${idx}`} src={att.url} alt={att.name} style={{ width: '100%', height: 70, objectFit: 'cover', borderRadius: 6 }} />
                      <div className="filename" title={att.name} style={{ fontSize: 12, marginTop: 6, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{att.name}</div>
                      <button type="button" data-testid={`remove-img-${idx}`} className="remove" onClick={() => requestRemove(idx)} style={{ position: 'absolute', top: 6, right: 6 }}>×</button>
                    </div>
                  ) : null)}
                </div>
              </div>

              <div className="evidence-card">
                <div style={{ fontSize: 13, color: '#0b1320', marginBottom: 8 }}>Video Snippets</div>
                <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
                  {defect.attachments.filter(a => a.type && a.type.startsWith && a.type.startsWith('video/')).length === 0 && (
                    <div style={{ fontSize: 12, color: '#475569' }}>No videos</div>
                  )}
                  {defect.attachments.map((att, idx) => att.type && att.type.startsWith && att.type.startsWith('video/') ? (
                    <div key={idx} className="video-preview">
                      <video data-testid={`video-${idx}`} src={att.url} controls style={{ width: '100%', borderRadius: 6, background: '#000' }} />
                      <div className="filename" title={att.name} style={{ fontSize: 12, marginTop: 6 }}>{att.name}</div>
                      <button type="button" data-testid={`remove-video-${idx}`} className="btn ghost" onClick={() => requestRemove(idx)} style={{ marginTop: 6 }}>Remove</button>
                    </div>
                  ) : null)}
                </div>
              </div>
            </div>

            <div style={{ marginTop: 10 }}>
              <div
                className={`file-drop${isDragging ? ' dragging' : ''}`}
                onDragOver={onDragOver}
                onDragEnter={onDragOver}
                onDragLeave={onDragLeave}
                onDrop={onDrop}
              >
                <div>
                  Drag and drop files here or
                  <label style={{ color: '#0b74ff', cursor: 'pointer', marginLeft: 6 }}>
                    <input data-testid="file-input" ref={fileInputRef} type="file" style={{ display: 'none' }} accept="image/*,video/*" onChange={onFileChange} multiple />
                    browse
                  </label>
                </div>
              </div>
            </div>

            <div className="voice">
              <div style={{ fontSize: 14, marginBottom: 8 }}>Voice Input</div>
              <button type="button" onClick={() => alert('start recording (placeholder)')}>Start Recording</button>
            </div>
          </div>
        </div>

        <aside className="right-column">
          <div className="meta">
            <div className="control">
              <label>Severity</label>
              <select value={defect.severity} onChange={(e) => updateField('severity', e.target.value)}>
                {getSeverityOptions().map(severity => (
                  <option key={severity} value={severity}>{severity}</option>
                ))}
              </select>
            </div>

            <div className="control">
              <label>Priority</label>
              <select value={defect.priority} onChange={(e) => updateField('priority', e.target.value)}>
                {getPriorityOptions().map(priority => (
                  <option key={priority} value={priority}>{priority}</option>
                ))}
              </select>
            </div>

            <div className="control">
              <label>Environment</label>
              <select value={defect.environment} onChange={(e) => updateField('environment', e.target.value)}>
                {getEnvironmentOptions().map(env => (
                  <option key={env} value={env}>{env}</option>
                ))}
              </select>
            </div>

            {status.success && <div style={{ color: '#16a34a', marginTop: 8 }}>{status.success}</div>}
            {status.error && <div style={{ color: '#dc2626', marginTop: 8 }}>{status.error}</div>}
          </div>
        </aside>
      </form>

      <ConfirmDialog
        open={confirm.open}
        title="Remove file"
        message={`Remove ${confirm.name}?`}
        onConfirm={handleConfirm}
        onCancel={handleCancel}
      />

      <div className="bottom-actions">
        <div />
        <div style={{ display: 'flex', gap: 8 }}>
          <button type="submit" className="btn primary" disabled={status.submitting}>{status.submitting ? 'Submitting…' : 'Submit Defect'}</button>
          <button className="btn primary" onClick={() => alert('cancel (placeholder)')}>Cancel</button>
        </div>
      </div>
    </div>
  );
}

export default DefectForm;
