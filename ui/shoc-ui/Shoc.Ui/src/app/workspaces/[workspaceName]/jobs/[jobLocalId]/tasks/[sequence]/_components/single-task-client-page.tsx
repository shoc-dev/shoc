"use client"

import useJobTask from "@/providers/job-task/use-job-task"
import JobTaskProgressAlert from "./job-task-progress-alert";
import JobTaskStatusBadge from "../../../_components/job-task-status-badge";
import useJob from "@/providers/job/use-job";
import { useIntl } from "react-intl";
import JobTaskActionsDropdown, { JobTaskActionTypes } from "./job-task-actions-dropdown";
import JobTaskExecutionSummaryCard from "./job-task-execution-summary-card";
import JobTaskResourcesCard from "./job-task-resources-card";
import { useMemo } from "react";
import Link from "next/link";
import { localDateTime } from "@/extended/format";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Logs } from "lucide-react";

export default function SingleTaskClientPage() {

    const intl = useIntl();
    const router = useRouter();
    const { value: task, load: loadTask, loading: taskLoading } = useJobTask();
    const { value: job, loading: jobLoading } = useJob();

    const loading = taskLoading || jobLoading;

    const onActionSelected = async (action: JobTaskActionTypes) => {
        if (action === 'refresh') {
            await loadTask()
        }

        if (action === 'view_logs') {
            router.push(`/workspaces/${task.workspaceName}/jobs/${job.localId}/tasks/${task.sequence}/logs`)
        }
    }

    const details = useMemo(() => !task ? [] : [
        {
            name: intl.formatMessage({id: 'jobs.tasks.task'}),
            value: <Link prefetch={false} className="underline" href={`/workspaces/${task.workspaceName}/jobs/${job?.localId}/tasks/${task.sequence}`}>{task.sequence}</Link>
        },
        {
            name: intl.formatMessage({id: 'jobs.job'}),
            value: <Link prefetch={false} className="underline" href={`/workspaces/${task.workspaceName}/jobs/${job?.localId}`}>{job?.localId}</Link>
        },
        {
            name: intl.formatMessage({id: 'jobs.labels.cluster'}),
            value: <Link prefetch={false} className="underline" href={`/workspaces/${task.workspaceName}/clusters/${task.clusterName}`}>{task.clusterName}</Link>
        },
        {
            name: intl.formatMessage({id: 'jobs.labels.package'}),
            value: task.packageTemplateReference
        },
        {
            name: intl.formatMessage({id: 'jobs.labels.actor'}),
            value: task.userFullName
        },
        {
            name: intl.formatMessage({id: 'global.labels.type'}),
            value: intl.formatMessage({id: `jobs.tasks.types.${task.type}`})
        },
        {
            name: intl.formatMessage({id: 'global.labels.status'}),
            value: intl.formatMessage({id: `jobs.statuses.${task.status}`})
        },
        {
            name: intl.formatMessage({id: 'global.labels.message'}),
            value: <span title={task.message}>{task.message || 'N/A'}</span>
        },
        {
            name: intl.formatMessage({id: 'global.labels.created'}),
            value: localDateTime(task.created)
        },
        {
            name: intl.formatMessage({id: 'jobs.labels.pendingAt'}),
            value: task.pendingAt ? localDateTime(task.pendingAt) : 'N/A'
        },
        {
            name: intl.formatMessage({id: 'jobs.labels.runningAt'}),
            value: task.runningAt ? localDateTime(task.runningAt) : 'N/A'
        },
        {
            name: intl.formatMessage({id: 'jobs.labels.completedAt'}),
            value: task.completedAt ? localDateTime(task.completedAt) : 'N/A'
        }
        
    ], [task, job?.localId, intl])

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
                <Button variant="outline" onClick={() => onActionSelected('view_logs')}><Logs /> <span className="hidden md:flex">{intl.formatMessage({id: 'jobs.logs.actions.view'})}</span></Button>
                <JobTaskActionsDropdown disabled={loading} onSelect={onActionSelected} />
            </div>
        </div>
        <div className="flex flex-col space-y-4">
            <JobTaskProgressAlert />
        </div>
        <div className="w-full bg-white p-4 shadow rounded-xl space-y-3 border">
            <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-4 text-sm text-gray-700">
                {details.map((item, index) => <div key={`description-${index}`} className="truncate">
                    <span className="font-semibold">{item.name}:</span> {item.value}
                </div>)}
            </div>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-1 lg:grid-cols-2 2xl:grid-cols-2 gap-4">
            <JobTaskExecutionSummaryCard />
            <JobTaskResourcesCard />
        </div>

        

    </div>
}