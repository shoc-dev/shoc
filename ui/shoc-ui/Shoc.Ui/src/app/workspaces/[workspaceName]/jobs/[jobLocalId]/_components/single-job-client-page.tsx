"use client"

import LoadingContainer from "@/components/general/loading-container";
import useJob from "@/providers/job/use-job"
import useWorkspace from "@/providers/workspace/use-workspace";
import { useIntl } from "react-intl";
import JobStatusBadge from "../../_components/job-status-badge";
import JobTasksSummaryCard from "./job-tasks-summary-card";
import JobExecutionSummaryCard from "./job-execution-summary-card";
import JobMetadataCard from "./job-metadata-card";
import JobActionsDropdown, { JobActionTypes } from "./job-actions-dropdown";
import JobTasksTable from "./job-tasks-table";

export default function SingleJobClientPage() {

    const intl = useIntl();
    const { loading: workspaceLoading } = useWorkspace();
    const { value: job, load: loadJob, loading: jobLoading } = useJob()

    const loading = workspaceLoading || jobLoading;

    const onActionSelected = async (action: JobActionTypes) => {

        if (action === 'refresh') {
            await loadJob()
        }
    }

    return <div className="space-y-4">
        <div className="flex items-center justify-between space-y-4">
            <div className="flex flex-col">
                <div className="flex flex-row space-x-2 items-center">
                    <h3 className="scroll-m-20 text-2xl font-semibold tracking-tight">
                        {job.name ? `${job.name} (${job.localId})` : `${intl.formatMessage({ id: 'jobs.job' })} ${job.localId}`}
                    </h3>
                    <JobStatusBadge status={job.status} />
                </div>

                <p className="text-muted-foreground text-balanced">
                    {job.description}
                </p>
            </div>
            <div className="space-x-2">
                <JobActionsDropdown disabled={loading} onSelect={onActionSelected} />
            </div>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-1 lg:grid-cols-2 2xl:grid-cols-3 gap-4">
            <JobMetadataCard />
            <JobExecutionSummaryCard />
            <JobTasksSummaryCard />
        </div>

        <div>
            <JobTasksTable />
        </div>
    </div>



}