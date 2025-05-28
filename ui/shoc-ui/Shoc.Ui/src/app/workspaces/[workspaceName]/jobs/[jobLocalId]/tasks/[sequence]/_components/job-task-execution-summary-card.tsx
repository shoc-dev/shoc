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
import { Play } from "lucide-react";
import { durationBetween } from "@/extended/format";
import useJobTask from "@/providers/job-task/use-job-task";

export default function JobTaskExecutionSummaryCard() {

  const intl = useIntl();
  const { value: task, loading: taskLoading } = useJobTask();

  return <LoadingContainer className="" loading={taskLoading}>
    <Card className="h-full">
      <CardHeader>
        <CardTitle><span className="flex"><Play className="w-5 h-5 mr-2" /> {intl.formatMessage({ id: 'jobs.tasks.execution.summary.title' })}</span></CardTitle>
        <CardDescription>{intl.formatMessage({ id: 'jobs.tasks.execution.summary.description' })}</CardDescription>
      </CardHeader>
      <CardContent className="flex pb-0">
        <div className={cn("grid grid-cols-2 gap-4 md:grid-cols-3")}>
          <div className="border-l pl-2 flex flex-col justify-center text-left">
            <span className="text-xs text-muted-foreground">{intl.formatMessage({ id: 'jobs.labels.waiting' })}</span>
            <span className="text-md font-semibold xl:text-xl">{task ? durationBetween(task.created, task.runningAt) : 'N/A'} </span>
          </div>
          <div className="border-l pl-2 flex flex-col justify-center text-left">
            <span className="text-xs text-muted-foreground">{intl.formatMessage({ id: 'jobs.labels.running' })}</span>
            <span className="text-md font-semibold xl:text-xl">{task?.runningAt ? durationBetween(task.runningAt, task.completedAt) : 'N/A'}
            </span>
          </div>
          <div className="border-l pl-2 flex flex-col justify-center text-left">
            <span className="text-xs text-muted-foreground">{intl.formatMessage({ id: 'jobs.labels.overall' })}</span>
            <span className="text-md font-semibold xl:text-xl">{task ? durationBetween(task.created, task.completedAt) : 'N/A'}
            </span>
          </div>
        </div>
      </CardContent>
    </Card>
  </LoadingContainer>
}