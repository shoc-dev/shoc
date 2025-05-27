import { Alert, AlertTitle } from "@/components/ui/alert";
import { JobStatus } from "@/domain/job";
import useJob from "@/providers/job/use-job";
import { useMemo } from "react";
import { useIntl } from "react-intl";


export default function JobProgressAlert() {

    const intl = useIntl();
    const { value: job } = useJob();

    const alerts: Record<JobStatus, any> = useMemo(() => ({
        'created': {
            className: 'text-gray-800 rounded-lg bg-gray-50 dark:bg-gray-800 dark:text-gray-400',
        },
        'pending': {
            className: 'text-amber-800 rounded-lg bg-amber-50 dark:bg-gray-800 dark:text-amber-400',
        },
        'running': {
            className: 'text-blue-800 rounded-lg bg-blue-50 dark:bg-gray-800 dark:text-blue-400',
        },
        'partially_succeeded': {
            className: 'text-indigo-800 rounded-lg bg-indigo-50 dark:bg-gray-800 dark:text-indigo-400',
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

    const alert = useMemo(() => alerts[job.status], [alerts, job.status]);
    const messages = useMemo(() => ({
        title: intl.formatMessage({id: `jobs.statuses.${job.status}.alert.title`}),
        description: intl.formatMessage({id: `jobs.statuses.${job.status}.alert.description`})
    }), [intl, job.status])

    return <Alert variant='default' className={alert.className}>
        <AlertTitle>{messages.title}</AlertTitle>
        {messages.description && <div className="col-start-2 grid justify-items-start gap-1 text-sm [&_p]:leading-relaxed">
            {messages.description}
        </div>}
        {job.message && <div className="col-start-2 grid justify-items-start gap-1 text-sm [&_p]:leading-relaxed truncate">
           {intl.formatMessage({id: 'global.labels.message'})}: {job.message}
        </div>}
    </Alert>
}