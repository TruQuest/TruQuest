import { MoralisProvider } from "react-moralis";
import Home from "./Home";

const App = () => {
  return (
    <MoralisProvider initializeOnMount={false}>
      <Home />
    </MoralisProvider>
  );
};

export default App;
