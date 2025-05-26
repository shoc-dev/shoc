"use client"

import { Badge } from "@/components/ui/badge";
import { JobTaskStatus } from "@/domain/job";
import { useIntl } from "react-intl";

const statusClasses: Record<JobTaskStatus, string> = {
    created: "bg-gray-100 text-gray-800 hover:bg-gray-100 border border-gray-300",
    pending: "bg-amber-100 text-amber-800 hover:bg-amber-100 border border-amber-300",
    running: "bg-blue-100 text-blue-800 hover:bg-blue-100 border border-blue-300",
    succeeded: "bg-green-100 text-green-800 hover:bg-green-100 border border-green-300",
    failed: "bg-red-100 text-red-800 hover:bg-red-100 border border-red-300",
    cancelled: "bg-gray-200 text-gray-800 hover:bg-gray-200 border border-gray-400"
  }

export default function JobTaskStatusBadge({ status }: { status: JobTaskStatus }){
    
    const intl = useIntl();

    return <Badge className={statusClasses[status]}>
        {intl.formatMessage({id: `jobs.statuses.${status}`})}
    </Badge>

}