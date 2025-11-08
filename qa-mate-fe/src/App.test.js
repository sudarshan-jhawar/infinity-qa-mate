import { render, screen } from '@testing-library/react';
import App from './App';

test('renders defect form heading', () => {
  render(<App />);
  const heading = screen.getByText(/log new defect/i);
  expect(heading).toBeInTheDocument();
});
