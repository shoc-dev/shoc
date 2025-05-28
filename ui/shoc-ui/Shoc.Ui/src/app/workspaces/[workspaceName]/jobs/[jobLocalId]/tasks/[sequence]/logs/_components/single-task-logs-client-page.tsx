"use client"

import useJobTask from "@/providers/job-task/use-job-task"
import useJob from "@/providers/job/use-job";
import { useIntl } from "react-intl";
import JobTaskStatusBadge from "../../../../_components/job-task-status-badge";
import JobTaskLogsActionsDropdown, { JobTaskLogsActionTypes } from "./job-task-logs-actions-dropdown";
import JobTaskProgressAlert from "../../_components/job-task-progress-alert";
import { useRouter } from "next/navigation";
import CodeBlock from "@/components/vc/code-block";

export default function SingleTaskLogsClientPage() {

    const intl = useIntl();
    const { value: task, loading: taskLoading } = useJobTask();
    const { loading: jobLoading } = useJob();
    const router = useRouter();

    const loading = taskLoading || jobLoading;

    const onActionSelected = async (action: JobTaskLogsActionTypes) => {
        if (action === 'refresh') {
            router.refresh()
        }
    }

    return <div className="space-y-4">
        <div className="flex items-center justify-between space-y-4">
            <div className="flex flex-col">
                <div className="flex flex-row space-x-2 items-center">
                    <h3 className="scroll-m-20 text-2xl font-semibold tracking-tight">
                        {intl.formatMessage({ id: 'jobs.logs' })} - {intl.formatMessage({ id: 'jobs.tasks.task' })} {task.sequence}
                    </h3>
                    <JobTaskStatusBadge status={task.status} />
                </div>

            </div>
            <div className="space-x-2">
                <JobTaskLogsActionsDropdown disabled={loading} onSelect={onActionSelected} />
            </div>
        </div>
        <div className="flex flex-col space-y-4">
            <JobTaskProgressAlert />
        </div>
        <div className="flex">
            Logs will go here
        </div>

    </div>
}