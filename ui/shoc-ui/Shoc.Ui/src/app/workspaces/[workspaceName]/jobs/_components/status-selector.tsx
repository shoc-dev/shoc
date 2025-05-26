import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select";
import { JobStatus } from "@/domain/job";
import { useIntl } from "react-intl";

const options: JobStatus[] = [
    'created',
    'pending',
    'running',
    'partially_succeeded',
    'succeeded',
    'failed',
    'cancelled',
]

export default function StatusSelector({ className, value, onChange, disabled }: { className?: string, value?: string, onChange: (value?: string) => void, disabled?: boolean }) {

    const intl = useIntl();

    return <Select
        value={value}
        onValueChange={onChange}
        disabled={disabled}
    >
        <SelectTrigger className={className}>
            <SelectValue placeholder={intl.formatMessage({ id: 'jobs.filters.status.placeholder' })} />
        </SelectTrigger>
        <SelectContent>
            <SelectItem key='all' value='all'>
                {intl.formatMessage({ id: 'jobs.filters.status.prefix' })} {intl.formatMessage({ id: 'global.filters.all' })}
            </SelectItem>
            {options.map((option) => (
                <SelectItem key={option} value={option}>
                    {intl.formatMessage({ id: 'jobs.filters.status.prefix' })} { intl.formatMessage({ id: `jobs.statuses.${option}` }) }
                </SelectItem>
            ))}
        </SelectContent>
    </Select>
}