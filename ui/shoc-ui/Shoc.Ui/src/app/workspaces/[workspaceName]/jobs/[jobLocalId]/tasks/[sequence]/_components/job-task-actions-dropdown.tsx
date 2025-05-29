import { Button } from "@/components/ui/button";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { Logs, MoreHorizontal, RefreshCcw } from "lucide-react";
import { useIntl } from "react-intl";

export type JobTaskActionTypes = 'refresh' | 'view_logs'

type JobTaskActionsDropdownProps = {
    disabled?: boolean
    onSelect: (action: JobTaskActionTypes) => void
}

export default function JobTaskActionsDropdown({ onSelect, disabled }: JobTaskActionsDropdownProps) {

    const intl = useIntl();

    return <DropdownMenu>
        <DropdownMenuTrigger asChild disabled={disabled}>
            <Button variant="outline">
                <MoreHorizontal className="ml-auto" /> 
                <span className="sm:inline hidden">{intl.formatMessage({ id: 'global.labels.actions' })}</span>
            </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent
            side="bottom"
            align="end"
            className="min-w-56 rounded-lg"
        >
            <DropdownMenuItem onClick={() => onSelect('refresh')}>
                <RefreshCcw />
                {intl.formatMessage({ id: 'global.actions.refresh' })}
            </DropdownMenuItem>
            <DropdownMenuItem onClick={() => onSelect('view_logs')}>
                <Logs />
                {intl.formatMessage({ id: 'jobs.logs.actions.view' })}
            </DropdownMenuItem>
        </DropdownMenuContent>
    </DropdownMenu>
}