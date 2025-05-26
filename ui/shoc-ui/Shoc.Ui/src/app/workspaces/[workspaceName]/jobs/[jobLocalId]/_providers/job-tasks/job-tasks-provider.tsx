"use client"

import { useCallback, useEffect, useMemo, useState } from "react";
import { rpc } from "@/server-actions/rpc";
import useJob from "@/providers/job/use-job";
import JobTasksContext from "./job-tasks-context";

export default function JobTasksProvider({  children }: { children: React.ReactNode }) {

    const { value: job } = useJob();
    const [progress, setProgress] = useState<boolean>(true)
    const [result, setResult] = useState<{ data: any, errors: any[] }>({ data: null, errors: [] })

    const load = useCallback(async () => {
        setProgress(true);

        const { data, errors } = await rpc('job/workspace-job-tasks/getAll', { 
            workspaceId: job.workspaceId,
            jobId: job.id
        })

        if (errors) {
            setResult({ data: null, errors })
        } else {
            setResult({ data: data, errors: [] })
        }

        setProgress(false);
    }, [job.workspaceId, job.id])

    useEffect(() => {
        
        if(job){
            load()
        } 
    }, [load, job])

    const value = useMemo(() => ({
        value: result.data,
        load,
        loading: progress,
        errors: result.errors
    }), [load, progress, result.data, result.errors])

    
    return <JobTasksContext.Provider value={value}>
        {children}
    </JobTasksContext.Provider>
}