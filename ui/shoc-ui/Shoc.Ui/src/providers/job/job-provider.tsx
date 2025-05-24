"use client"

import { useCallback, useMemo, useState } from "react";
import JobContext, { JobValueType } from "./job-context";
import { rpc } from "@/server-actions/rpc";

const mapper = (value: any): JobValueType => {
    return {
        ...value
    }
}

export default function JobProvider({ children, initialValue }: { children: React.ReactNode, initialValue: any }) {

    const [progress, setProgress] = useState<boolean>(false)
    const [result, setResult] = useState<{ data: any, errors: any[] }>({ data: initialValue, errors: [] })

    const load = useCallback(async () => {
        setProgress(true);

        const { data, errors } = await rpc('job/workspace-jobs/getByLocalId', { workspaceId: initialValue.workspaceId, localId: initialValue.localId })

        if (errors) {
            setResult({ data: null, errors })
        } else {
            setResult({ data: data, errors: [] })
        }

        setProgress(false);

    }, [initialValue.workspaceId, initialValue.name])

    const initialValueMapped = useMemo(() => mapper(initialValue), [initialValue]);
    const valueMapped = useMemo(() => mapper(result.data ?? initialValue), [result.data, initialValue])

    const value = useMemo(() => ({
        initialValue: initialValueMapped,
        value: valueMapped,
        load,
        loading: progress,
        errors: result.errors
    }), [initialValueMapped, valueMapped, load, progress, result.errors])
    
    return <JobContext.Provider value={value}>
        {children}
    </JobContext.Provider>
}