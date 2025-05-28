"use client"

import useJobTask from "@/providers/job-task/use-job-task"
import JobTaskProgressAlert from "./job-task-progress-alert";
import JobTaskStatusBadge from "../../../_components/job-task-status-badge";
import useJob from "@/providers/job/use-job";
import { useIntl } from "react-intl";
import JobTaskActionsDropdown, { JobTaskActionTypes } from "./job-task-actions-dropdown";

export default function SingleTaskClientPage() {
    
    const intl = useIntl();
    const { value: task, load: loadTask, loading: taskLoading } = useJobTask();
    const { loading: jobLoading } = useJob();

    const loading = taskLoading || jobLoading;

    const onActionSelected = async (action: JobTaskActionTypes) => {
        if (action === 'refresh') {
            await loadTask()
        }
    }

    return <div className="space-y-4">
        <div className="flex items-center justify-between space-y-4">
            <div className="flex flex-col">
                <div className="flex flex-row space-x-2 items-center">
                    <h3 className="scroll-m-20 text-2xl font-semibold tracking-tight">
                        {intl.formatMessage({ id: 'jobs.tasks.task' })} {task.sequence}
                    </h3>
                    <JobTaskStatusBadge status={task.status} />
                </div>

            </div>
            <div className="space-x-2">
                <JobTaskActionsDropdown disabled={loading} onSelect={onActionSelected} />
            </div>
        </div>
        <div className="flex flex-col space-y-4">
            <JobTaskProgressAlert />
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-1 lg:grid-cols-2 2xl:grid-cols-3 gap-4">
            {/* <JobMetadataCard />
            <JobExecutionSummaryCard />
            <JobTasksSummaryCard /> */}
        </div>


    </div>
}