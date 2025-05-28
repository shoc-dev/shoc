import { Alert, AlertTitle } from "@/components/ui/alert";
import { JobTaskStatus } from "@/domain/job";
import useJobTask from "@/providers/job-task/use-job-task";
import { useMemo } from "react";
import { useIntl } from "react-intl";

export default function JobTaskProgressAlert() {

    const intl = useIntl();
    const { value: task } = useJobTask();

    const alerts: Record<JobTaskStatus, any> = useMemo(() => ({
        'created': {
            className: 'text-gray-800 rounded-lg bg-gray-50 dark:bg-gray-800 dark:text-gray-400',
        },
        'pending': {
            className: 'text-amber-800 rounded-lg bg-amber-50 dark:bg-gray-800 dark:text-amber-400',
        },
        'running': {
            className: 'text-blue-800 rounded-lg bg-blue-50 dark:bg-gray-800 dark:text-blue-400',
        },
        'succeeded': {
            className: 'text-green-800 rounded-lg bg-green-50 dark:bg-gray-800 dark:text-green-400',
        },
        'failed': {
            className: 'text-red-800 rounded-lg bg-red-50 dark:bg-gray-800 dark:text-red-400',
        },
        'cancelled': {
            className: 'text-gray-800 rounded-lg bg-gray-100 dark:bg-gray-800 dark:text-gray-400',
        }
    }), []);

    const alert = useMemo(() => alerts[task.status], [alerts, task.status]);
    const messages = useMemo(() => ({
        title: intl.formatMessage({id: `jobs.tasks.statuses.${task.status}.alert.title`}),
        description: intl.formatMessage({id: `jobs.tasks.statuses.${task.status}.alert.description`})
    }), [intl, task.status])

    return <Alert variant='default' className={alert.className}>
        <AlertTitle>{messages.title}</AlertTitle>
        {messages.description && <div className="col-start-2 grid justify-items-start gap-1 text-sm [&_p]:leading-relaxed">
            {messages.description}
        </div>}
        {task.message && task.status === 'failed' && <div className="col-start-2 grid justify-items-start gap-1 text-sm [&_p]:leading-relaxed truncate">
           {intl.formatMessage({id: 'global.labels.message'})}: {task.message}
        </div>}
    </Alert>
}