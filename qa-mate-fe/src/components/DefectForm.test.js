import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { DefectProvider } from '../context/DefectContext';
import DefectForm from './DefectForm';

function renderWithProvider(ui) {
  return render(<DefectProvider>{ui}</DefectProvider>);
}

describe('DefectForm attachments', () => {
  beforeEach(() => {
    // clear mocks between tests
    jest.clearAllMocks();
  });

  beforeAll(() => {
    // jsdom doesn't implement createObjectURL; mock for previews
    if (!global.URL.createObjectURL) {
      global.URL.createObjectURL = (file) => `blob:${file.name}`;
    }
    if (!global.URL.revokeObjectURL) {
      global.URL.revokeObjectURL = jest.fn();
    }
  });

  test('adds image attachment and renders thumbnail', async () => {
    renderWithProvider(<DefectForm />);
    // enter a non-duplicate title and blur so the component enables attachments
    const titleInput = screen.getByLabelText('Defect Title');
    await userEvent.type(titleInput, 'Unique test title');
    fireEvent.blur(titleInput);

    // wait until description becomes enabled (title check finished)
    await waitFor(() => expect(screen.getByLabelText('Description')).not.toBeDisabled());

    const fileInput = screen.getByTestId('file-input');
    const file = new File(['(⌐□_□)'], 'very_long_image_name_example_screenshot.png', { type: 'image/png' });

    // simulate selecting file
    await waitFor(() => {
      fireEvent.change(fileInput, { target: { files: [file] } });
    });

    // image should appear
    const img = await screen.findByAltText('very_long_image_name_example_screenshot.png');
    expect(img).toBeInTheDocument();

    // filename should be present as truncated text with title attribute
    const filename = screen.getByTitle('very_long_image_name_example_screenshot.png');
    expect(filename).toBeInTheDocument();
  });

  test('adds video attachment and renders preview', async () => {
    renderWithProvider(<DefectForm />);
    const titleInput = screen.getByLabelText('Defect Title');
    await userEvent.type(titleInput, 'Another unique title');
    fireEvent.blur(titleInput);
    await waitFor(() => expect(screen.getByLabelText('Description')).not.toBeDisabled());

    const fileInput = screen.getByTestId('file-input');
    const file = new File(['dummy'], 'sample_video_clip.mp4', { type: 'video/mp4' });

    fireEvent.change(fileInput, { target: { files: [file] } });

    const video = await screen.findByTestId('video-0');
    expect(video).toBeInTheDocument();
  });

  test('removes attachment after confirmation', async () => {
    renderWithProvider(<DefectForm />);
    const titleInput = screen.getByLabelText('Defect Title');
    await userEvent.type(titleInput, 'Title to remove');
    fireEvent.blur(titleInput);
    await waitFor(() => expect(screen.getByLabelText('Description')).not.toBeDisabled());

    const fileInput = screen.getByTestId('file-input');
    const file = new File(['(⌐□_□)'], 'to_remove.png', { type: 'image/png' });

    fireEvent.change(fileInput, { target: { files: [file] } });

    const img = await screen.findByAltText('to_remove.png');
    expect(img).toBeInTheDocument();

    // click remove which opens modal
    const removeBtn = screen.getByTestId('remove-img-0');
    userEvent.click(removeBtn);

    // dialog should appear
    const dialog = await screen.findByTestId('confirm-dialog');
    expect(dialog).toBeInTheDocument();

    // click OK
    const ok = screen.getByTestId('confirm-ok');
    userEvent.click(ok);

    await waitFor(() => {
      expect(screen.queryByAltText('to_remove.png')).not.toBeInTheDocument();
    });
  });
});
