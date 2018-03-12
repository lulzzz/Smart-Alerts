// -----------------------------------------------------------------------
// <copyright file="signalResultsView.tsx" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

import * as React from 'react';
import { connect } from 'react-redux';
import { bindActionCreators, Dispatch } from 'redux';
import { Grid } from 'react-flexbox-grid';
import { DataTable, TableHeader, TableBody, TableRow, TableColumn, TablePagination } from 'react-md/lib/DataTables';
import CircularProgress from 'react-md/lib/Progress/CircularProgress';
import { withRouter, RouteComponentProps } from 'react-router-dom';
import { Moment } from 'moment';
import * as moment from 'moment';

import Drawer from '../Drawer';
import StoreState, { SignalsResultsStoreState, ResourcesStoreState } from '../../store/StoreState';
import SignalResult from '../../models/SignalResult';
import { getSignalsResults } from '../../actions/signalResult/signalResultActions';
import { SignalResultUtils } from '../../utils/SignalResultUtils';
import SignalResultDetailsDrawerView from './signalResultDetailsDrawerView';
import SignalResultProperty from '../../models/SignalResultProperty';
import { ChartMetadataUtils } from '../../utils/ChartMetadataUtils';
import { SignalResultPropertyUtils } from '../../utils/SignalResultPropertyUtils';
import ChartMetadata from '../../models/ChartMetadata';
import { getAzureResources } from '../../actions/resource/resourceActions';

import './signalResultViewStyle.css';

/**
 * Represents the SignalResultView component props for the dispatch functions
 */
interface SignalResultViewDispatchProps {
    getSignalResult: (startTime: Moment) => (dispatch: Dispatch<StoreState>) => Promise<void>;
    getAzureResources: () => (dispatch: Dispatch<StoreState>) => Promise<void>;
}

/**
 * Represents the SignalResultView component props for the component inner state  
 */
interface SignalResultViewStateProps {
    signalResults: SignalsResultsStoreState;
    resources: ResourcesStoreState;
}

/**
 * Represents the SignalResultView component props for the incoming properties
 */
interface SignalResultViewOwnProps extends RouteComponentProps<{}> {
    selectedSignalResultId?: string;
}

// Create a type combined from all the props
type SignalResultViewProps = SignalResultViewDispatchProps &
                             SignalResultViewStateProps &
                             SignalResultViewOwnProps;

/**
 * Represents the SignalResultView component state
 */
interface SignalResultViewState {
    showSignalResultDetailsDrawer: boolean;
    selectedSignalResultId?: string;
    slicedData: ReadonlyArray<SignalResult>;
}

class SignalResultView extends React.Component<SignalResultViewProps, SignalResultViewState> {
    private NumberOfSignalResultsPerPage = 10;

    constructor(props: SignalResultViewProps) {
        super(props);

        this.state = {
            showSignalResultDetailsDrawer: props.selectedSignalResultId !== undefined,
            selectedSignalResultId: undefined,
            slicedData: new Array()
        };
    }

    public async componentDidMount() {
        // We are getting the signal results from the last hour (as this it the default)
        await this.props.getSignalResult(moment.utc().add(-1, 'hour'));

        await this.props.getAzureResources();
    }

    public componentWillReceiveProps(nextProps: Readonly<SignalResultViewProps>) {
        this.setState({ slicedData: nextProps.signalResults.items.slice(0, this.NumberOfSignalResultsPerPage)});
    }

    public render() {
        return (
            <div>
                <Grid fluid className="signal-results-view-container">
                    <div className="page-title">
                        Smart Signals Results
                    </div>

                    <div className="signal-results-presentation-and-filters">
                        <div className="filters-container">
                            <div className="text-before-select-box-filter">
                                * Subscription
                            </div>
                            <select className="select-box">
                                {
                                    ((this.props.signalResults &&
                                    this.props.signalResults.isFetching) ||
                                    (this.props.resources &&
                                     this.props.resources.isFetching)) &&
                                    <option disabled selected>Loading...</option>
                                }
                                {
                                    this.props.signalResults &&
                                    this.props.resources &&
                                    !this.props.signalResults.isFetching &&
                                    !this.props.signalResults.failureReason &&
                                    this.props.resources.azureSubscriptionsResources.map(subscriptionResources => 
                                        (
                                            <option value={subscriptionResources.subscription.id}>
                                                {subscriptionResources.subscription.displayName}
                                            </option>
                                        ))
                                }
                            </select>
                            <div className="text-before-select-box-filter">
                                * Time range
                            </div>
                            <select className="select-box" onChange={this.onTimeRangeSelectionChanged}>
                                <option value="1">Last 1 hour</option>
                                <option value="6">Last 6 hours</option>
                                <option value="24">Last day</option>
                                <option value="168">Last 7 days</option>
                                <option value="336">Last 14 days</option>
                            </select>
                        </div>
                        <DataTable selectableRows={false} className="signal-results-table"> 
                            <TableHeader>
                                <TableRow>
                                    <TableColumn>Timestamp</TableColumn>
                                    <TableColumn>Signal name</TableColumn>
                                    <TableColumn>Alert type</TableColumn>
                                    <TableColumn>Target resource</TableColumn>
                                    <TableColumn>Monitor service</TableColumn>
                                    <TableColumn>Resource type</TableColumn> 
                                    <TableColumn>Alert criteria</TableColumn>
                                </TableRow>
                            </TableHeader>
                            {
                                !this.props.signalResults.isFetching &&
                                <TableBody>
                                    {
                                        this.state.slicedData.map((signalResult, index) => (
                                            <TableRow key={index} onClick={this.onSignalResultRowClick}>
                                                <TableColumn>{signalResult.analysisTimestamp}</TableColumn>
                                                <TableColumn>{signalResult.signalName}</TableColumn>
                                                <TableColumn>Smart Signal</TableColumn>
                                                <TableColumn>
                                                    {SignalResultUtils.getResourceName(signalResult.resourceId)}
                                                </TableColumn>
                                                <TableColumn>Azure monitor</TableColumn>
                                                <TableColumn>Application Insights</TableColumn>
                                                <TableColumn>{signalResult.title}</TableColumn>
                                            </TableRow>
                                        ))
                                    }
                                </TableBody>
                            }
                            {
                                !this.props.signalResults.isFetching &&
                                <TablePagination 
                                    rows={this.props.signalResults.items.length} 
                                    onPagination={this.handlePagination} 
                                    rowsPerPageLabel="Results per page"
                                    rowsPerPage={this.NumberOfSignalResultsPerPage}
                                    className="table-pagination"
                                    defaultRowsPerPage={this.NumberOfSignalResultsPerPage}
                                />
                            }
                        </DataTable>
                        {
                            this.props.signalResults.isFetching &&
                            <div className="loading-signal-results">
                                <CircularProgress id="view-signal-results-progress" />
                            </div>
                        }
                        </div>
                </Grid>

                <Drawer 
                    visible={this.state.showSignalResultDetailsDrawer}
                    onVisibilityChange={this.onSignalResultDetailsDrawerVisibilityChange}
                    maxWidth="33%"
                    minWidth="33%"
                >
                    {
                        // First we are taking the selected signal result from the state - in case we came to this
                        // page from specific signal result and then the user changed one
                        this.state.selectedSignalResultId &&
                        this.getSignalResultDetailsDrawerContent(this.state.selectedSignalResultId)
                    }
                    
                    {
                        // Show signal result details from the props only if state is empty
                        !this.state.selectedSignalResultId && 
                        this.props.selectedSignalResultId &&
                        this.getSignalResultDetailsDrawerContent(this.props.selectedSignalResultId)
                    }
                </Drawer>
            </div>
        );
    }

    private onTimeRangeSelectionChanged = async (event: React.FormEvent<HTMLSelectElement>) => {
        await this.props.getSignalResult(moment.utc().add(-Number(event.currentTarget.value), 'hour'));
    }

    private handlePagination = (start: number, rowsPerPage: number) => {
        this.setState({ slicedData: this.props.signalResults.items.slice(start, start + rowsPerPage) });
    }

    private onSignalResultRowClick = (event: React.MouseEvent<HTMLTableRowElement>) => {
        let selectedRowNumber: number = event.currentTarget.rowIndex;
        let selectedSignalResult: SignalResult = this.props.signalResults.items[selectedRowNumber - 1];

        if (this.props.history) {
            this.props.history.push(`signalResults/${selectedSignalResult.id}`);
        }

        this.setState({ showSignalResultDetailsDrawer: true, selectedSignalResultId: selectedSignalResult.id });
    }

    private onSignalResultDetailsDrawerVisibilityChange = (visible: boolean) => {
        // Go back in history - remove the signal id from the url
        this.props.history.goBack();

        this.setState({ showSignalResultDetailsDrawer: visible, selectedSignalResultId: undefined });
    }

    private getSignalResultDetailsDrawerContent(signalResultId: string): JSX.Element {
        let signalResultsById = this.props.signalResults.items.filter(result => result.id === signalResultId);

        // Check we found the signal result
        if (signalResultsById.length !== 1) {
            throw new Error('Failed to find signal result');
        }

        let selectedSignalResult = signalResultsById[0];

        let allChartsProperties = SignalResultUtils.getAllChartProperties(selectedSignalResult);
        let chartsMetadata = this.getChartsMetadata(selectedSignalResult,
                                                    allChartsProperties);

        return (
            <SignalResultDetailsDrawerView 
                signalResult={selectedSignalResult}
                chartsMetadata={chartsMetadata}
            />
        );
    }

    private getChartsMetadata(signalResult: SignalResult, chartsProperties: SignalResultProperty[]): ChartMetadata[] {
        return chartsProperties.map(property => {
            return {
                id: ChartMetadataUtils.createChartId(signalResult.id, property.name, property.value),
                name: property.name,
                query: property.value,
                chartType: SignalResultPropertyUtils.getChartTypeFromProperty(property),
                queryRunInfo: signalResult.queryRunInfo
            } as ChartMetadata;
        }).filter(chartMetadata => chartMetadata.query !== '');
    }
}

/**
 * Map between the given state to this component props.
 * @param state The current state
 */
function mapStateToProps(state: StoreState): SignalResultViewStateProps {
    return {
        signalResults: state.signalsResults,
        resources: state.resources
    };
}

/**
 * Map between the given dispatch to this component props actions.
 * @param dispatch the dispatch
 */
function mapDispatchToProps(dispatch: Dispatch<StoreState>): SignalResultViewDispatchProps {
    return {
        getSignalResult: bindActionCreators(getSignalsResults, dispatch),
        getAzureResources: bindActionCreators(getAzureResources, dispatch)
    };
}

// Casting to any in order to prevent the competability issue with Redux - Router
// tslint:disable-next-line:no-any
export default withRouter(connect(mapStateToProps, mapDispatchToProps)(SignalResultView));