import React, { ReactNode } from 'react';
import { AssertionModal } from './controls/Modals/AssertionModal';

interface GlobalErrorBoundaryProps {
    children: ReactNode;
}

interface GlobalErrorBoundaryState {
    isError: boolean;
}

class GlobalErrorBoundary extends React.Component<GlobalErrorBoundaryProps, GlobalErrorBoundaryState> {
    constructor(props: GlobalErrorBoundaryProps) {
        super(props);

        this.state = {
            isError: false,
        };
    }

    public static getDerivedStateFromError(error: Error): GlobalErrorBoundaryState {
        return { isError: true };
    }

    public render() {
        if (!this.state.isError) {
            return this.props.children;
        }

        return (
            <div>
                {this.state.isError && (
                    <AssertionModal
                        title="Упс..."
                        message="Произошла непредвиденная ошибка"
                        assertTitle="Обновить страницу"
                        onAssert={() => location.reload()}
                        onClose={() => location.reload()}
                    />
                )}
                {this.props.children}
            </div>
        );
    }
}

export default GlobalErrorBoundary;
