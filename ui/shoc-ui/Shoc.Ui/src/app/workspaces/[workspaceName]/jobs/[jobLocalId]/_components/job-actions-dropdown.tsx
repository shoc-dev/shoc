import { Button } from "@/components/ui/button";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { MoreHorizontal, RefreshCcw } from "lucide-react";
import { useIntl } from "react-intl";

export type JobActionTypes = 'refresh'

type ClusterActionsDropdownProps = {
    disabled?: boolean
    onSelect: (action: JobActionTypes) => void
}

export default function JobActionsDropdown({ onSelect, disabled }: ClusterActionsDropdownProps) {

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
        </DropdownMenuContent>
    </DropdownMenu>
}