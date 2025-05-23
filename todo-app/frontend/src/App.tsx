import React from 'react';
import { BrowserRouter as Router, Route, Switch } from 'react-router-dom';
import HomePage from './pages/HomePage';
import TodoListPage from './pages/TodoListPage';
import SharedListPage from './pages/SharedListPage';

const App: React.FC = () => {
  return (
    <Router>
      <Switch>
        <Route path="/" exact component={HomePage} />
        <Route path="/todo/:id" component={TodoListPage} />
        <Route path="/shared/:id" component={SharedListPage} />
      </Switch>
    </Router>
  );
};

export default App;