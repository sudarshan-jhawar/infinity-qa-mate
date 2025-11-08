import './App.css';
import { DefectProvider } from './context/DefectContext';
import DefectForm from './components/DefectForm';
import ChatBotButton from './components/ChatBotButton';

function App() {
  return (
    <div className="App">
      <DefectProvider>
        <DefectForm />
        <ChatBotButton />
      </DefectProvider>
    </div>
  );
}

export default App;
