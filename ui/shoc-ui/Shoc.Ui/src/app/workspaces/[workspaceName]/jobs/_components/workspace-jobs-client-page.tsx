"use client"

import { useCallback, useEffect, useState } from "react"
import ErrorScreen from "@/components/error/error-screen";
import { rpc } from "@/server-actions/rpc";
import { useIntl } from "react-intl";
import NoJobs from "./no-jobs";
import JobsTable from "./jobs-table";
import { Button } from "@/components/ui/button";
import { ChevronLeft, ChevronRight } from "lucide-react";
import LoadingContainer from "@/components/general/loading-container";
import ScopeSelector from "./scope-selector";
import StatusSelector from "./status-selector";
import { FilterOptions, JobScope, JobStatus } from "@/domain/job";

const DEFAULT_PAGE_SIZE = 10;

export default function WorkspaceJobsClientPage({ workspaceId, workspaceName }: any) {

    const [progress, setProgress] = useState(true);
    const [data, setData] = useState<any>(null);
    const [errors, setErrors] = useState<any[]>([]);

    const [filter, setFilter] = useState<FilterOptions>({
        scope: undefined,
        page: 0,
        size: DEFAULT_PAGE_SIZE
    });

    const intl = useIntl();

    const load = useCallback(async (workspaceId: string, filter: FilterOptions) => {
        setProgress(true);
        const { data, errors } = await rpc('job/workspace-jobs/getBy', { workspaceId, filter: { ...filter } })

        if (errors) {
            setErrors(errors);
            setData(null);
        } else {
            setErrors([]);
            setData(data)
        }

        setProgress(false);
    }, [])

    useEffect(() => {

        if (!workspaceId) {
            return;
        }

        load(workspaceId, filter)

    }, [workspaceId, load, filter])

    if (errors && errors.length > 0) {
        return <ErrorScreen errors={errors} />
    }

    return <>
        <div className="flex flex-row space-x-2">
            <ScopeSelector className="w-[150px]" value={filter.scope || 'all'} onChange={newValue => setFilter(prev => ({
                ...prev,
                scope: newValue !== 'all' ? newValue as JobScope : undefined,
                page: 0
            }))} disabled={progress} />
            <StatusSelector className="w-[220px]" value={filter.status || 'all'} onChange={newValue => setFilter(prev => ({
                ...prev,
                status: newValue !== 'all' ? newValue as JobStatus : undefined,
                page: 0
            }))} disabled={progress} />
        </div>
        <LoadingContainer className="w-full h-full m-auto mt-4" loading={progress}>
        {(data?.totalCount === 0) && <NoJobs className="w-full h-full" workspaceId={workspaceId} />}
            {(!data || data.totalCount > 0) &&
                <div className="flex flex-col">
                    <JobsTable className="mt-4" workspaceName={workspaceName} items={data?.items || []} />
                    {data && data.totalCount > filter.size && <div className="flex mt-2 ml-auto space-x-2">
                        <Button variant="outline" disabled={filter.page === 0} onClick={() => setFilter((prev: any) => ({
                            ...prev,
                            page: prev.page - 1
                        }))}>
                            <ChevronLeft className="mr-2 w-4 h-4" />
                            {intl.formatMessage({ id: 'global.navigation.prev' })}
                        </Button>
                        <Button variant="outline" disabled={filter.page + 1 >= data?.totalCount / filter.size} onClick={() => setFilter((prev: any) => ({
                            ...prev,
                            page: prev.page + 1
                        }
                        ))}>
                            {intl.formatMessage({ id: 'global.navigation.next' })}
                            <ChevronRight className="ml-2 w-4 h-4" />
                        </Button>
                    </div>
                    }
                </div>
            }
        </LoadingContainer>
    </>
}