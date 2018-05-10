import * as React from 'react';
import { RouteComponentProps } from 'react-router';

interface HomeState {
    schemaList: string[];
    loading: boolean;
}

export class Home extends React.Component<RouteComponentProps<{}>, HomeState> {
    constructor(props: any) {
        super();
        this.state = { schemaList: [], loading: true };

        fetch('api/DBList/SchemaList')
            .then(response => response.json() as Promise<string[]>)
            .then(data => {
                this.setState({ schemaList: data, loading: false });
            });
    }

    public render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : Home.RenderDbTable(this.state.schemaList);

        return <div>
            <h1>DataLoadReadReview</h1>
            <p>Schema List:</p>
            {contents}
        </div>;
    }

    private static RenderDbTable(tables: string[]) {
        return <ul>
            {tables.map((table: string) =>
                <li key={table}>
                    <a href={`/${table}`}>
                        {table}
                    </a>
                </li>
            )}
        </ul>;
    }
}
