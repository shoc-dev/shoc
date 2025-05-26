"use client"

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { useIntl } from "react-intl"
import LoadingContainer from "@/components/general/loading-container";
import { cn } from "@/lib/utils";
import { FolderCheck } from "lucide-react";
import useJob from "@/providers/job/use-job";

export default function JobTasksSummaryCard() {

  const intl = useIntl();
  const { value: job, loading: jobLoading } = useJob();

  return <LoadingContainer className="" loading={jobLoading}>
    <Card className="h-full">
      <CardHeader>
        <CardTitle><span className="flex"><FolderCheck className="w-5 h-5 mr-2" /> {intl.formatMessage({ id: 'jobs.tasks.summary.title' })}</span></CardTitle>
        <CardDescription>{intl.formatMessage({ id: 'jobs.tasks.summary.description' })}</CardDescription>
      </CardHeader>
      <CardContent className="flex pb-0">
        <div className={cn("grid grid-cols-3 gap-4 md:grid-cols-5")}>
          <div className="border-l pl-2 flex flex-col justify-center text-left">
            <span className="text-xs text-muted-foreground">{intl.formatMessage({ id: 'jobs.labels.completedTasks' })}</span>
            <span className="text-lg font-semibold xl:text-2xl">{job?.completedTasks ?? 0} </span>
          </div>
          <div className="border-l pl-2 flex flex-col justify-center text-left">
            <span className="text-xs text-muted-foreground">{intl.formatMessage({ id: 'jobs.labels.totalTasks' })}</span>
            <span className="text-lg font-semibold xl:text-2xl">{job?.totalTasks ?? 0} </span>
          </div>
          <div className="border-l pl-2 flex flex-col justify-center text-left">
            <span className="text-xs text-muted-foreground">{intl.formatMessage({ id: 'jobs.statuses.succeeded' })}</span>
            <span className="text-lg font-semibold xl:text-2xl">{job?.succeededTasks ?? 0} </span>
          </div>
          <div className="border-l pl-2 flex flex-col justify-center text-left">
            <span className="text-xs text-muted-foreground">{intl.formatMessage({ id: 'jobs.statuses.failed' })}</span>
            <span className="text-lg font-semibold xl:text-2xl">{job?.failedTasks ?? 0} </span>
          </div>
          <div className="border-l pl-2 flex flex-col justify-center text-left">
            <span className="text-xs text-muted-foreground">{intl.formatMessage({ id: 'jobs.statuses.cancelled' })}</span>
            <span className="text-lg font-semibold xl:text-2xl">{job?.cancelledTasks ?? 0} </span>
          </div>
        </div>
      </CardContent>
    </Card>
  </LoadingContainer>
}