import * as React from 'react';
import { Route, Switch } from 'react-router-dom';

import App from './components/App';

/**
 * The main routing component which our application starts from.
 */
export default (
  <Switch>
    <Route component={App} />
  </Switch>
);