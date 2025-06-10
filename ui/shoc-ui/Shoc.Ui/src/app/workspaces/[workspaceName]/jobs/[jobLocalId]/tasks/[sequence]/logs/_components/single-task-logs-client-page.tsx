"use client"

import useJobTask from "@/providers/job-task/use-job-task"
import useJob from "@/providers/job/use-job";
import { useIntl } from "react-intl";
import JobTaskStatusBadge from "../../../../_components/job-task-status-badge";
import JobTaskLogsActionsDropdown, { JobTaskLogsActionTypes } from "./job-task-logs-actions-dropdown";
import JobTaskProgressAlert from "../../_components/job-task-progress-alert";
import { Textarea } from "@/components/ui/textarea";
import { useCallback, useEffect, useMemo, useState } from "react";
import ErrorDefinitions from "@/addons/error-handling/error-definitions";
import LoadingContainer from "@/components/general/loading-container";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";

export default function SingleTaskLogsClientPage() {

    const intl = useIntl();
    const { value: task, loading: taskLoading } = useJobTask();
    const { loading: jobLoading } = useJob();
    const [connecting, setConnecting] = useState(true);
    const [errors, setErrors] = useState<any[]>([]);
    const [logs, setLogs] = useState('');

    const loading = taskLoading || jobLoading;

    const onActionSelected = async (action: JobTaskLogsActionTypes) => {
        if (action === 'refresh') {
            load()
        }
    }

    const url = useMemo(
        () => `/api/workspaces/${task.workspaceId}/jobs/${task.jobId}/tasks/${task.sequence}/logs`,
        [task.workspaceId, task.jobId, task.sequence]
    )

    const getLogs = useCallback(async (url: string) => {
        setErrors([]);
        setLogs('')
        setConnecting(true);

        const response = await fetch(url);

        setConnecting(false)
        if (!response.ok) {

            try {
                const errorBody = await response.json();
                setErrors(errorBody.errors)
            }
            catch (e: any) {
                setErrors([ErrorDefinitions.unknown(e.message)])
            }
            return;
        }

        if (!response.body) {
            return;
        }

        const reader = response.body.getReader();
        const decoder = new TextDecoder();

        while (true) {
            const { done, value } = await reader.read();

            if (done) {
                break;
            }

            const chunk = decoder.decode(value, { stream: true });

            const events = chunk.split('\n\n');
            for (const event of events) {
                if (event.startsWith('data: ')) {
                    const eventData = event.slice(6);
                    setLogs(prev => `${prev}${eventData}\n`);
                }
            }
        }
    }, []);

    const load = useCallback(async () => {
        await getLogs(url);
    }, [url, getLogs])


    useEffect(() => {
        load()
    }, [load])


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
        {errors?.length > 0 && <Alert variant='destructive'>
            <AlertTitle>{intl.formatMessage({id: 'jobs.logs.errors.notAvailable'})}</AlertTitle>
            {errors[0].message && <AlertDescription>
                {errors[0].message}    
            </AlertDescription>}
            </Alert>}
        <div className="flex h-full">
            <LoadingContainer loading={connecting} className="h-full w-full">
                <Textarea className="h-full" 
                value={logs} 
                placeholder={intl.formatMessage({id: 'jobs.logs.placeholder'})} 
                disabled={loading || connecting || errors?.length > 0} 
                readOnly />
            </LoadingContainer>
        </div>

    </div>
}