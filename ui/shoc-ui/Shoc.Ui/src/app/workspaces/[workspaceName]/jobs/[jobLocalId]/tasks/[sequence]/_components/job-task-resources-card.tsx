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
import { Cpu } from "lucide-react";
import { bytesToSize } from "@/extended/format";
import useJobTask from "@/providers/job-task/use-job-task";

export default function JobTaskResourcesCard() {

  const intl = useIntl();
  const { value: task, loading: taskLoading } = useJobTask();

  return <LoadingContainer className="" loading={taskLoading}>
    <Card className="h-full">
      <CardHeader>
        <CardTitle><span className="flex"><Cpu className="w-5 h-5 mr-2" /> {intl.formatMessage({ id: 'jobs.tasks.resources.title' })}</span></CardTitle>
        <CardDescription>{intl.formatMessage({ id: 'jobs.tasks.resources.description' })}</CardDescription>
      </CardHeader>
      <CardContent className="flex pb-0">
        <div className={cn("grid grid-cols-3 gap-4")}>
          <div className="border-l pl-2 flex flex-col justify-center text-left">
            <span className="text-xs text-muted-foreground">{intl.formatMessage({ id: 'jobs.resources.types.cpu' })}</span>
            <span className="text-md font-semibold xl:text-xl">{task?.cpuRequested ?? 'N/A'} </span>
          </div>
          <div className="border-l pl-2 flex flex-col justify-center text-left">
            <span className="text-xs text-muted-foreground">{intl.formatMessage({ id: 'jobs.resources.types.memory' })}</span>
            <span className="text-md font-semibold xl:text-xl">{bytesToSize(task?.memoryRequested ?? 0)}
            </span>
          </div>
          <div className="border-l pl-2 flex flex-col justify-center text-left">
            <span className="text-xs text-muted-foreground">{intl.formatMessage({ id: 'jobs.resources.types.nvidiaGpu' })}</span>
            <span className="text-md font-semibold xl:text-xl">{task?.nvidiaGpuRequested ?? 0}
            </span>
          </div>
        </div>
      </CardContent>
    </Card>
  </LoadingContainer>
}