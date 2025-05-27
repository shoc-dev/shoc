"use client"

import useJobTask from "@/providers/job-task/use-job-task"

export default function SingleTaskClientPage(){
    const { value: task } = useJobTask();
    return <pre>
        {JSON.stringify(task, null, 4)}
    </pre>
}