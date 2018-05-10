import * as React from 'react';
import { RouteComponentProps } from 'react-router';
import 'isomorphic-fetch';

interface DBListState {
    dbList: string[];
    loading: boolean;
    schema: string;
    uploadProgress: {};
}

export class DBList extends React.Component<RouteComponentProps<{}>, DBListState> {
    constructor(props: any) {
        super();
        this.state = {
            dbList: [], schema: props.match.params.dbName, loading: true, uploadProgress: {} };

        fetch(`api/DBList/DbList/${props.match.params.dbName}`)
            .then(response => response.json() as Promise<any>)
            .then(data => {
                if (data.error) {
                    console.log(data.error);
                } else {
                    this.setState({ dbList: data.payload, loading: false });
                }
            });
    }

    updateProgress(tableName: string, value: string) {
        this.setState({
            uploadProgress: {
                ...this.state.uploadProgress,
                [tableName]: value
            }
        });
    }

    public render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : DBList.RenderDbTable(this.state.dbList, this.state.schema, this.state.uploadProgress, this.updateProgress.bind(this));

        return <div>
            <h1>Table List:</h1>
            {contents}
        </div>;
    }

    public static onUploadClicked(dbName: string, tableName: string, cb: any) {
        cb(tableName, 'Uploading...');
        fetch(`api/DBList/UploadToGCS/${dbName}/${tableName}`)
            .then(response => response.json() as Promise<any>)
            .then(data => {
                if (data.error) {
                    cb(tableName, `Error: ${data.error}`);
                } else {
                    cb(tableName, `Done. Link: (${data.payload.selfLink})`);
                }

            });
    }

    private static RenderTableList(props: any) {
        const secondRowStyle = {
            margin: '10px'
        }
        const resultStyle = {
            marginLeft: '20px'
        }

        return <div className="list-group">
            {props.tables.map((table: string) => {
                return <div
                        className="list-group-item"
                        key={table}
                    >
                        <b className="list-group-item-heading">{table}</b>
                        <div className="list-group-item-text" style={secondRowStyle}>
                            <button
                                onClick={DBList.onUploadClicked.bind(this, props.db, table, props.cb)}
                                className="btn btn-default">Upload</button>
                        <span style={resultStyle}>{props.uploadProgress[table]}</span>
                        </div>
                    </div>
                }
            )}
        </div>;
    }

    private static RenderEmptyTableList() {
        return <p>No tables found!</p>;
    }

    private static RenderDbTable(tables: string[], db: string, uploadProgress: any, cb: any) {
        if (tables.length > 0)
            return <DBList.RenderTableList tables={tables} db={db} uploadProgress={uploadProgress} cb={cb} />;
        else
            return <DBList.RenderEmptyTableList />
    }
}