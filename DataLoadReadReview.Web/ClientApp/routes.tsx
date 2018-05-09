import * as React from 'react';
import { Route } from 'react-router-dom';
import { Layout } from './components/Layout';
import { DBList } from './components/DBList';
import { Home } from './components/Home';

export const routes = <Layout>
    <Route exact path='/' component={ Home } />
    <Route exact path='/:dbName' component={ DBList } />
</Layout>;
