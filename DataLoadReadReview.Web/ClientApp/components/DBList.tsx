import * as React from 'react';
import { RouteComponentProps } from 'react-router';
import 'isomorphic-fetch';

interface DBListState {
    dbList: string[];
    loading: boolean;
    schema: string;
}

export class DBList extends React.Component<RouteComponentProps<{}>, DBListState> {
    constructor(props: any) {
        super();
        this.state = { dbList: [], schema: props.match.params.dbName, loading: true };

        fetch(`api/DBList/DbList/${props.match.params.dbName}`)
            .then(response => response.json() as Promise<string[]>)
            .then(data => {
                this.setState({ dbList: data, loading: false });
            });
    }

    public render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : DBList.RenderDbTable(this.state.dbList, this.state.schema);

        return <div>
            <h1>Table List:</h1>
            {contents}
        </div>;
    }

    public static onUploadClicked(dbName: string, tableName: string) {
        fetch(`api/DBList/UploadToGCS/${dbName}/${tableName}`)
            .then(response => response.json() as Promise<any>)
            .then(data => {
                console.log(data)
            })
            .catch(err => console.log(err));
    }

    private static RenderTableList(props: any) {
        return <div className="list-group">
            {props.tables.map((table: string) =>
                <div
                    className="list-group-item"
                    key={table}
                >
                    <div className="col-md-6">{table}</div>
                    <button
                        onClick={DBList.onUploadClicked.bind(this, props.db, table)}
                        className="btn btn-default">Upload</button>
                    <div className="col-md-1 col-md-offset-2">Result</div>
                </div>
            )}
        </div>;
    }

    private static RenderEmptyTableList() {
        return <p>No tables found!</p>;
    }

    private static RenderDbTable(tables: string[], db: string) {
        if (tables.length > 0)
            return <DBList.RenderTableList tables={tables} db={db} />;
        else
            return <DBList.RenderEmptyTableList />
    }
}