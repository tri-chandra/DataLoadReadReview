import * as React from 'react';
import { RouteComponentProps } from 'react-router';
import 'isomorphic-fetch';

interface DBListState {
    dbList: string[];
    loading: boolean;
    schema: string;
    uploadProgress: {};
    metadata: {};
    errorMessage: string
}

export class DBList extends React.Component<RouteComponentProps<{}>, DBListState> {
    constructor(props: any) {
        super();
        this.state = {
            dbList: [], schema: props.match.params.dbName, loading: true, uploadProgress: {}, metadata: {}, errorMessage: '' };

        fetch(`api/DBList/DbList/${props.match.params.dbName}`)
            .then(response => response.json() as Promise<any>)
            .then(data => {
                if (data.error) {
                    this.setState({ errorMessage: data.error });
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

    updateMeta(tableName: string, value: string[]) {
        this.setState({
            metadata: {
                ...this.state.metadata,
                [tableName]: value
            }
        });
    }

    public render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : DBList.RenderDbTable(
                this.state.dbList,
                this.state.schema,
                this.state.uploadProgress,
                this.state.metadata,
                this.updateProgress.bind(this),
                this.updateMeta.bind(this));

        return <div>
            <h1>Table List:</h1>
            {this.state.errorMessage &&
                <div className="alert alert-danger" role="alert">{this.state.errorMessage}</div>
            }
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

    public static onGetMetaClicked(dbName: string, tableName: string, cb: any) {
        fetch(`api/DBList/GetMeta/${dbName}/${tableName}`)
            .then(response => response.json() as Promise<any>)
            .then(data => {
                if (data.error) {

                } else {
                    cb(tableName, data.payload.map((value: any) => {
                        return {
                            link: value.selfLink,
                            size: value.size,
                            lastModified: value.updated
                        };
                    }));
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
        const glyphStyle = {
            marginRight: '10px'
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
                            onClick={DBList.onUploadClicked.bind(this, props.db, table, props.progressCb)}
                                className="btn btn-default">Upload</button>
                            <button
                                onClick={DBList.onGetMetaClicked.bind(this, props.db, table, props.metaCb)}
                                className="btn btn-default">Get Metadata</button>
                            <span style={resultStyle}>{props.uploadProgress[table]}</span>

                        {props.metadata[table] &&
                            (props.metadata[table].length > 0
                            ? props.metadata[table].map((item: any) => {
                                return <p key={item.link} style={{ marginTop: '20px' }} >
                                    <span className="glyphicon glyphicon-file" style={glyphStyle}></span>
                                    {item.link}
                                    <br />
                                    <span className="glyphicon glyphicon-time" style={glyphStyle}></span>
                                    { Math.round(100*item.size/1024)/100 } KB
                                    <br />
                                    <span className="glyphicon glyphicon-hdd" style={glyphStyle}></span>
                                    {new Date(item.lastModified.toString()).toLocaleString()}
                                    <div style={{ borderBottom: 'solid', borderWidth: 'thin' }}></div>
                                </p>
                            })
                            : (() => { return <p>No file exists!</p> })())
                        }

                        </div>
                    </div>
                }
            )}
        </div>;
    }

    private static RenderEmptyTableList() {
        return <p>No tables found!</p>;
    }

    private static RenderDbTable(tables: string[], db: string, uploadProgress: any, metadata: any, progressCb: any, metaCb: any) {
        if (tables.length > 0)
            return <DBList.RenderTableList
                tables={tables}
                db={db}
                uploadProgress={uploadProgress}
                metadata={metadata}
                progressCb={progressCb}
                metaCb={metaCb}
            />;
        else
            return <DBList.RenderEmptyTableList />
    }
}