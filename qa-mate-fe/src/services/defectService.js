import { API_CONFIG, getApiUrl, DEFAULT_FETCH_OPTIONS } from '../config/apiConfig';

/**
 * Check if a defect title already exists
 * @param {string} title - The defect title to check
 * @returns {Promise<boolean>} - True if title exists, false otherwise
 */
export const checkDefectTitleExists = async (title) => {
  try {
    // TODO: Replace with actual API call
    // const response = await fetch(`${API_BASE_URL}/defects/exists?title=${encodeURIComponent(title)}`);
    // if (!response.ok) throw new Error('Failed to check title');
    // const data = await response.json();
    // return data.exists;

    // Simulation for development
    await new Promise(resolve => setTimeout(resolve, 600));
    return /duplicate|exists|existing|found|same/i.test(title);
  } catch (error) {
    console.error('Error checking defect title:', error);
    throw new Error('Failed to check if defect title exists');
  }
};

/**
 * Submit a new defect
 * @param {Object} defect - The defect data to submit
 * @returns {Promise<Object>} - The created defect
 */
export const submitDefect = async (defect) => {
  try {
    // TODO: Replace with actual API call
    // const response = await fetch(`${API_BASE_URL}/defects`, {
    //   method: 'POST',
    //   headers: {
    //     'Content-Type': 'application/json',
    //   },
    //   body: JSON.stringify(defect),
    // });
    
    // if (!response.ok) throw new Error('Failed to submit defect');
    // return await response.json();

    // Simulation for development
    await new Promise(resolve => setTimeout(resolve, 800));
    
    // Log the defect data that would be sent to the API
    console.log('Submitting Defect:', {
      Title: defect.title,
      Description: defect.description,
      Steps: defect.steps,
      Severity: defect.severity,
      Priority: defect.priority,
      Environment: defect.environment,
      Attachments: defect.attachments.map(a => ({ name: a.name, type: a.type }))
    });

    return { ok: true, id: Date.now(), ...defect };
  } catch (error) {
    console.error('Error submitting defect:', error);
    throw new Error('Failed to submit defect');
  }
};

/**
 * Upload file attachments for a defect
 * @param {string} defectId - The ID of the defect
 * @param {File[]} files - Array of files to upload
 * @returns {Promise<Object[]>} - Array of uploaded file metadata
 */
export const uploadDefectAttachments = async (defectId, files) => {
  try {
    // TODO: Replace with actual API call
    // const formData = new FormData();
    // files.forEach(file => formData.append('files', file));
    
    // const response = await fetch(`${API_BASE_URL}/defects/${defectId}/attachments`, {
    //   method: 'POST',
    //   body: formData,
    // });
    
    // if (!response.ok) throw new Error('Failed to upload attachments');
    // return await response.json();

    // Simulation for development
    await new Promise(resolve => setTimeout(resolve, 400));
    return files.map(file => ({
      id: Date.now(),
      name: file.name,
      type: file.type,
      url: URL.createObjectURL(file)
    }));
  } catch (error) {
    console.error('Error uploading attachments:', error);
    throw new Error('Failed to upload attachments');
  }
};