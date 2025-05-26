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
import { Info, Link2, Play } from "lucide-react";
import useJob from "@/providers/job/use-job";
import Link from "next/link";

export default function JobMetadataCard() {

  const intl = useIntl();
  const { value: job, loading: jobLoading } = useJob();

  return <LoadingContainer className="" loading={jobLoading}>
    <Card className="h-full">
      <CardHeader>
        <CardTitle><span className="flex"><Info className="w-5 h-5 mr-2" /> {intl.formatMessage({ id: 'jobs.details.title' })}</span></CardTitle>
        <CardDescription>{intl.formatMessage({ id: 'jobs.details.description' })}</CardDescription>
      </CardHeader>
      <CardContent className="flex pb-0">
        <div className={cn("grid grid-cols-2 gap-4 md:grid-cols-2")}>
          <div className="border-l pl-2 flex flex-col justify-center text-left" title={job?.clusterName}>
            <span className="text-xs text-muted-foreground">{intl.formatMessage({ id: 'jobs.labels.cluster' })}</span>
            <span className="text-md font-semibold xl:text-xl truncate underline">
              {job?.clusterName ? <Link  target="_blank" href={`/workspaces/${job.workspaceName}/clusters/${job.clusterName}`}>
                {job.clusterName}
              </Link> : "N/A"}
            </span>
          </div>
          <div className="border-l pl-2 flex flex-col justify-center text-left" title={job?.userFullName}>
            <span className="text-xs text-muted-foreground">{intl.formatMessage({ id: 'jobs.labels.actor' })}</span>
            <span className="text-md font-semibold xl:text-xl truncate">
              {job?.userFullName ? job.userFullName : "N/A"}
            </span>
          </div>
        </div>
      </CardContent>
    </Card>
  </LoadingContainer>
}