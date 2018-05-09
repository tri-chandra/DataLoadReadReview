import * as React from 'react';
import { RouteComponentProps } from 'react-router';

export class Home extends React.Component<RouteComponentProps<{}>, {}> {
    public render() {
        return <div>
            <h1>DataLoadReadReview</h1>
            <p>Schema List:</p>
            <ul>
                <li><a href="/public">public</a></li>
                <li><a href="/information_schema">information_schema</a></li>
                <li><a href="/pg_catalog">pg_catalog</a></li>
            </ul>
        </div>;
    }
}
