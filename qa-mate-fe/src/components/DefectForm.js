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

  // title existence check state
  const [checkingTitle, setCheckingTitle] = useState(false);
  const [titleExists, setTitleExists] = useState(false);
  const [titleChecked, setTitleChecked] = useState(false);
  const [lastCheckedTitle, setLastCheckedTitle] = useState('');

  const requestRemove = (index) => {
    const item = defect.attachments[index];
    setConfirm({ open: true, index, name: item?.name || 'this file' });
  };

  const handleConfirm = () => {
    if (confirm.index !== null) removeAttachment(confirm.index);
    setConfirm({ open: false, index: null, name: '' });
  };

  const handleCancel = () => setConfirm({ open: false, index: null, name: '' });

  // form is valid when title, description and steps are non-empty
  const isFormValid = Boolean(defect.title && defect.title.trim() && defect.description && defect.description.trim() && defect.steps && defect.steps.trim());

  // helper to simulate or call an API to check if title exists
  const checkTitleExists = async (title) => {
    const t = title && title.trim();
    if (!t) {
      setTitleExists(false);
      setTitleChecked(false);
      setLastCheckedTitle('');
      return false;
    }

    // avoid re-checking same title
    if (lastCheckedTitle === t) {
      return titleExists;
    }

    setCheckingTitle(true);
    setTitleExists(false);
    setTitleChecked(false);

    // Simulated API call - replace with real fetch in production
    // e.g. const res = await fetch(`/api/defects/exists?title=${encodeURIComponent(t)}`)
    // const { exists } = await res.json();
    await new Promise((r) => setTimeout(r, 600));
    const exists = /duplicate|exists|existing|found|same/i.test(t);

    setCheckingTitle(false);
    setTitleExists(exists);
    setTitleChecked(!exists);
    setLastCheckedTitle(t);
    return exists;
  };

  const handleTitleBlur = () => {
    // run check on blur
    if (!defect.title || !defect.title.trim()) {
      setTitleExists(false);
      setTitleChecked(false);
      setLastCheckedTitle('');
      return;
    }
    // fire-and-forget
    checkTitleExists(defect.title);
  };

  const handleTitleChange = (val) => {
    // clear previous checks when title is modified
    updateField('title', val);
    setTitleChecked(false);
    setTitleExists(false);
    // don't clear lastCheckedTitle here; will be overwritten on next check
  };

  return (
    <div className="defect-card">
      {checkingTitle && (
        <div className="title-info checking">
          <div className="info">Checking title...</div>
        </div>
      )}
      {titleExists && (
        <div className="title-info exists">
          <div className="info">A defect with this title already exists. Please modify the title or search existing defects.</div>
        </div>
      )}
      {/* global status messages (moved here so they appear under the title-check banners) */}
      {status.success && (
        <div style={{ margin: '0 20px 12px' }}>
          <div style={{ color: '#16a34a', padding: '8px 10px', borderRadius: 6, background: '#f0fff4' }}>{status.success}</div>
        </div>
      )}
      {status.error && (
        <div style={{ margin: '0 20px 12px' }}>
          <div style={{ color: '#dc2626', padding: '8px 10px', borderRadius: 6, background: '#fff5f5' }}>{status.error}</div>
        </div>
      )}
      
      <div className="defect-header">
        <h3 className="defect-title">Log New Defect</h3>
      </div>

      <form className="defect-body" onSubmit={onSubmit}>
        <div className="left-column">
          <div className="field">
            <label htmlFor="title">Defect Title</label>
            <input id="title" type="text" value={defect.title} onChange={(e) => handleTitleChange(e.target.value)} onBlur={handleTitleBlur} required />
            {errors.title && <div style={{ color: '#dc2626', fontSize: 13 }}>{errors.title}</div>}
          </div>

          <div className="field">
            <label htmlFor="description">Description</label>
            <textarea id="description" value={defect.description} onChange={(e) => updateField('description', e.target.value)} required disabled={!titleChecked || titleExists} />
            {errors.description && <div style={{ color: '#dc2626', fontSize: 13 }}>{errors.description}</div>}
          </div>

          <div className="field">
            <label htmlFor="steps">Steps to Reproduce</label>
            <textarea id="steps" value={defect.steps} onChange={(e) => updateField('steps', e.target.value)} required disabled={!titleChecked || titleExists} />
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
                onDrop={(e) => { if (!titleChecked || titleExists) return; onDrop(e); }}
              >
                <div>
                  <div>Drag and drop files here or</div>
                  <label>
                    <input data-testid="file-input" ref={fileInputRef} type="file" style={{ display: 'none' }} accept="image/*,video/*" onChange={(e) => { if (!titleChecked || titleExists) return; onFileChange(e); }} multiple />
                    browse
                  </label>
                </div>
              </div>
            </div>

            <div className="field voice">
              <label>Voice Input</label>
              <button type="button" onClick={() => alert('start recording (placeholder)')}>Start Recording</button>
            </div>

            {/* meta moved back to right column */}
          </div>
        </div>

        <aside className="right-column">
          <div className="meta">
            <div className="control">
              <label>Severity</label>
              <select value={defect.severity} onChange={(e) => updateField('severity', e.target.value)} disabled={!titleChecked || titleExists}>
                {getSeverityOptions().map(severity => (
                  <option key={severity} value={severity}>{severity}</option>
                ))}
              </select>
            </div>

            <div className="control">
              <label>Priority</label>
              <select value={defect.priority} onChange={(e) => updateField('priority', e.target.value)} disabled={!titleChecked || titleExists}>
                {getPriorityOptions().map(priority => (
                  <option key={priority} value={priority}>{priority}</option>
                ))}
              </select>
            </div>

            <div className="control">
              <label>Environment</label>
              <select value={defect.environment} onChange={(e) => updateField('environment', e.target.value)} disabled={!titleChecked || titleExists}>
                {getEnvironmentOptions().map(env => (
                  <option key={env} value={env}>{env}</option>
                ))}
              </select>
            </div>

          </div>
        </aside>

        <div className="bottom-actions">
          <div />
          <div style={{ display: 'flex', gap: 8 }}>
            <button type="submit" className="btn primary" disabled={status.submitting || !isFormValid || titleExists || !titleChecked} title={!titleChecked ? 'Please enter title and click outside the field to check for duplicates' : titleExists ? 'A defect with this title already exists' : (!isFormValid ? 'Please fill Title, Description and Steps' : undefined)}>{status.submitting ? 'Submitting…' : 'Submit Defect'}</button>
            <button type="button" className="btn primary" onClick={() => resetDefect()} disabled={status.submitting}>Cancel</button>
          </div>
        </div>

      </form>

      <ConfirmDialog
        open={confirm.open}
        title="Remove file"
        message={`Remove ${confirm.name}?`}
        onConfirm={handleConfirm}
        onCancel={handleCancel}
      />
      
    </div>
  );
}

export default DefectForm;
