"use client"

import { useCallback, useMemo, useState } from "react";
import { rpc } from "@/server-actions/rpc";
import { JobValueType } from "@/domain/job";
import JobTaskContext from "./job-task-context";

const mapper = (value: any): JobValueType => {
    return {
        ...value
    }
}

export default function JobTaskProvider({ children, initialValue }: { children: React.ReactNode, initialValue: any }) {

    const [progress, setProgress] = useState<boolean>(false)
    const [result, setResult] = useState<{ data: any, errors: any[] }>({ data: initialValue, errors: [] })

    const load = useCallback(async () => {
        setProgress(true);
        const { data, errors } = await rpc('job/workspace-job-tasks/getBySequence', { workspaceId: initialValue.workspaceId, jobId: initialValue.jobId, sequence: initialValue.sequence })

        if (errors) {
            setResult({ data: null, errors })
        } else {
            setResult({ data: data, errors: [] })
        }

        setProgress(false);

    }, [initialValue.workspaceId, initialValue.jobId, initialValue.sequence])

    const initialValueMapped = useMemo(() => mapper(initialValue), [initialValue]);
    const valueMapped = useMemo(() => mapper(result.data ?? initialValue), [result.data, initialValue])

    const value = useMemo(() => ({
        initialValue: initialValueMapped,
        value: valueMapped,
        load,
        loading: progress,
        errors: result.errors
    }), [initialValueMapped, valueMapped, load, progress, result.errors])
    
    return <JobTaskContext.Provider value={value}>
        {children}
    </JobTaskContext.Provider>
}