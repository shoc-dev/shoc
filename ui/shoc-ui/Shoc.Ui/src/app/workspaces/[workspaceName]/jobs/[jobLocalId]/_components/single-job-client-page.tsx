"use client"

import LoadingContainer from "@/components/general/loading-container";
import useJob from "@/providers/job/use-job"
import useWorkspace from "@/providers/workspace/use-workspace";
import { useRouter } from "next/navigation";
import { useIntl } from "react-intl";
import JobStatusBadge from "../../_components/job-status-badge";

export default function SingleJobClientPage() {

    const intl = useIntl();
    const router = useRouter();
    const { loading: workspaceLoading } = useWorkspace();
    const { value: job, loading: jobLoading } = useJob()

    const loading = workspaceLoading || jobLoading;

    return <div className="space-y-4">
        <LoadingContainer loading={loading}>
            <div className="flex items-center justify-between space-y-4">
                <div className="flex flex-col">
                    <div className="flex flex-row space-x-2 items-center">
                        <h3 className="scroll-m-20 text-2xl font-semibold tracking-tight">
                            {job.name ? `${job.name} (${job.localId})` : `${intl.formatMessage({ id: 'jobs.job' })} ${job.localId}`}
                        </h3>
                        <JobStatusBadge status={job.status} />
                    </div>

                    <p className="text-muted-foreground text-balanced">
                        {job.description}
                    </p>
                </div>
                <div className="space-x-2">
                </div>
            </div>
        </LoadingContainer>


        <pre>
            {JSON.stringify(job, null, 4)}
        </pre>
    </div>



}